using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using AzureTraductor.Core;
using AzureTraductor.MVVM.Model;
using CsvHelper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AzureTraductor.MVVM.ViewModel
{
    class MainVM : ObservableObject
    {
        public ObservableCollection<Language> Languages { get; set; }
        public Language SelectedFromLanguage { get; set; }
        public Language SelectedToLanguage { get; set; }
        public Language SelectedToLanguagePdf { get; set; }

        private string _textToTranslate;
        private string _translatedText;
        private string _selectedFilePath;
        private string _translatedTextPdf;

        public string SelectedFilePath
        {
            get { return _selectedFilePath; }
            set
            {
                _selectedFilePath = value;
                OnPropertyChanged();
            }
        }

        public string TranslatedTextPdf
        {
            get { return _translatedTextPdf; }
            set
            {
                _translatedTextPdf = value;
                OnPropertyChanged();
            }
        }

        public string TextToTranslate
        {
            get { return _textToTranslate; }
            set
            {
                _textToTranslate = value;
                OnPropertyChanged();
            }
        }

        public string TranslatedText
        {
            get { return _translatedText; }
            set
            {
                _translatedText = value;
                OnPropertyChanged();
            }
        }

        public ICommand TranslateCommand { get; set; }
        public ICommand SelectFileCommand { get; }
        public ICommand TranslateCommandPdf { get; }

        private readonly string formRecognizerEndpoint;
        private readonly string formRecognizerApiKey;
        private readonly string translatorEndpoint;
        private readonly string translatorSubscriptionKey;
        private readonly string translatorRegion;

        public MainVM()
        {
            Languages = new ObservableCollection<Language>();
            LoadLanguagesFromCsv("Resources\\LanguagesList.csv");

            // Idiomas por defecto
            SelectedFromLanguage = Languages[109]; // Spanish
            SelectedToLanguage = Languages[27]; // English
            SelectedToLanguagePdf = Languages[27]; // English

            TranslateCommand = new RelayCommand(async o => await CheckText());
            SelectFileCommand = new RelayCommand(SelectFile);
            TranslateCommandPdf = new RelayCommand(async e => await TranslatePdf());

            // Cargar configuraciones desde appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            formRecognizerEndpoint = configuration["Azure:FormRecognizer:Endpoint"];
            formRecognizerApiKey = configuration["Azure:FormRecognizer:ApiKey"];
            translatorEndpoint = configuration["Azure:Translator:Endpoint"];
            translatorSubscriptionKey = configuration["Azure:Translator:SubscriptionKey"];
            translatorRegion = configuration["Azure:Translator:Region"];
        }

        private async Task CheckText()
        {
            if (SelectedFromLanguage != null && SelectedToLanguage != null && !string.IsNullOrEmpty(TextToTranslate))
            {
                (string translatedText, string originalText) = await TranslateText(TextToTranslate, SelectedFromLanguage.Code, SelectedToLanguage.Code);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TextToTranslate = originalText;
                    TranslatedText = translatedText;
                });
            }
            else
            {
                MessageBox.Show("Introduzca un texto a traducir.");
            }
        }

        // Método traductor de texto
        public async Task<(string translatedText, string originalText)> TranslateText(string textToTranslate, string fromLanguage, string toLanguage)
        {
            string route = $"/translate?api-version=3.0&from={fromLanguage}&to={toLanguage}";
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(translatorEndpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", translatorSubscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", translatorRegion);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();

                var jsonResponse = JArray.Parse(result);
                var translatedText = jsonResponse[0]["translations"][0]["text"].ToString();

                return (translatedText, textToTranslate);
            }
        }

        private void SelectFile(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.pdf)|*.pdf|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
            }
        }

        private async Task TranslatePdf()
        {
            if (!string.IsNullOrWhiteSpace(SelectedFilePath))
            {
                TranslatedTextPdf = "Translating PDF...";

                string targetFilePath = System.IO.Path.ChangeExtension(SelectedFilePath, "_translated.pdf");

                try
                {
                    // Extraer texto del PDF usando Azure Form Recognizer
                    string extractedText = await ExtractTextFromFileAsync(SelectedFilePath);

                    string detectedLanguage = await DetectLanguage(extractedText);
                    (string translatedText, string originalText) = await TranslateText(extractedText, detectedLanguage, SelectedToLanguagePdf.Code);

                    using (FileStream fs = new FileStream(targetFilePath, FileMode.Create))
                    {
                        using (Document doc = new Document())
                        {
                            PdfWriter.GetInstance(doc, fs);
                            doc.Open();
                            doc.Add(new Paragraph(translatedText));
                        }
                    }

                    TranslatedTextPdf = $"Translation successful. Translated file saved as {targetFilePath}";
                }
                catch (Exception ex)
                {
                    TranslatedTextPdf = $"Translation failed. Error: {ex.Message}";
                }
            }
            else
            {
                TranslatedTextPdf = "Please select a file first.";
            }
        }

        private async Task<string> ExtractTextFromFileAsync(string filePath)
        {
            var client = new DocumentAnalysisClient(new Uri(formRecognizerEndpoint), new AzureKeyCredential(formRecognizerApiKey));
            using var stream = new FileStream(filePath, FileMode.Open);
            var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", stream);
            var result = operation.Value;

            StringBuilder extractedText = new StringBuilder();
            foreach (var page in result.Pages)
            {
                foreach (var line in page.Lines)
                {
                    extractedText.AppendLine(line.Content);
                }
            }

            TranslatedTextPdf = "Text extracted successfully";
            return extractedText.ToString();
        }

        private async Task<string> DetectLanguage(string text)
        {
            string route = "/detect?api-version=3.0";
            object[] body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(translatorEndpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", translatorSubscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", translatorRegion);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();

                var jsonResponse = JArray.Parse(result);
                string detectedLanguage = jsonResponse[0]["language"].ToString();

                return detectedLanguage;
            }
        }

        private void LoadLanguagesFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Language>().ToList();
                foreach (var record in records)
                {
                    Languages.Add(record);
                }
            }
        }
    }
}

**README - English**

# Azure Translator App

Azure Translator App is a translation application built using C# and Azure services. It allows users to translate text and PDF files between different languages. The application utilizes Azure Form Recognizer for extracting text from PDFs and Azure Translator API for translation.

## Features

- **Text Translation**: Translate text between multiple languages using Azure Translator API.
- **PDF Translation**: Translate text extracted from PDF files into the desired language.
- **Automatic Language Detection**: Automatically detect the language of the text extracted from PDFs.
- **User Interface**: Simple and intuitive interface for selecting files and viewing translations.
- **Centralized Configuration**: Configuration settings such as API keys and endpoints are stored in an `appsettings.json` file for easy management.

## Getting Started

1. Clone the repository.
2. Open the solution in Visual Studio.
3. Restore NuGet packages.
4. Update the `appsettings.json` file with your Azure service credentials.
5. Ensure you have created Azure resources for Azure Translator and Document Intelligence.
6. Build and run the application.

## Requirements

- .NET Core 3.1 or higher
- Visual Studio 2019 or higher
- Azure resources for Azure Translator and Document Intelligence

## Usage

1. Launch the application.
2. Select the source language and the target language.
3. Enter the text to translate or select a PDF file.
4. Click the "Translate" button to view the translation.

## Contributors

- [Jose Pujante Ruiz](https://github.com/JosePujanteDev)

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

---
---

**README - Español**

# Aplicación de Traductor de Azure

La Aplicación de Traductor de Azure es una aplicación de traducción construida usando C# y servicios de Azure. Permite a los usuarios traducir texto y archivos PDF entre diferentes idiomas. La aplicación utiliza Azure Form Recognizer para extraer texto de los PDF y Azure Translator API para la traducción.

## Características

- **Traducción de Texto**: Traduce texto entre varios idiomas utilizando Azure Translator API.
- **Traducción de PDF**: Traduce el texto extraído de los archivos PDF al idioma deseado.
- **Detección Automática de Idioma**: Detecta automáticamente el idioma del texto extraído de los PDF.
- **Interfaz de Usuario**: Interfaz simple e intuitiva para seleccionar archivos y ver traducciones.
- **Configuración Centralizada**: La configuración, como claves de API y puntos de acceso, se almacena en un archivo `appsettings.json` para facilitar la gestión.

## Empezar

1. Clona el repositorio.
2. Abre la solución en Visual Studio.
3. Restaura los paquetes NuGet.
4. Actualiza el archivo `appsettings.json` con las credenciales de tus servicios de Azure.
5. Asegúrate de haber creado recursos de Azure para Azure Translator y Document Intelligence.
6. Compila y ejecuta la aplicación.

## Requisitos

- .NET Core 3.1 o superior
- Visual Studio 2019 o superior
- Recursos de Azure para Azure Translator y Document Intelligence

## Uso

1. Inicia la aplicación.
2. Selecciona el idioma de origen y el idioma de destino.
3. Ingresa el texto a traducir o selecciona un archivo PDF.
4. Haz clic en el botón "Traducir" para ver la traducción.

## Contribuyentes

- [Jose Pujante Ruiz](https://github.com/JosePujanteDev)

## Licencia

Este proyecto está bajo la [Licencia MIT](https://opensource.org/licenses/MIT).

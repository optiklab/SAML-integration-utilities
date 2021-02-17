# SAML-integration-utilities

### What it's all about

The aim is to show how to initialize Single Sign-On integration using [SAML version 2 standard]() from Identity Provider to Service Providers (or your web application with authentication mechanisms) using C# .NET 5.0. 

### How it works and what part of SAML workflow is covered

See my full artile on [dev.to](https://dev.to/optiklab/working-example-of-saml-single-sign-on-integration-using-c-39mb) to understand full context.

## Components

## Metadata File Generator

Small application that allows to initialize integration with Service Provider by generating and sending metadata.xml file.

### How to use

1. First of all, you have to buy X.509 certificate (or maybe you already have one) in one of the publicly known vendors. This certificate should include both Private and Public keys and allow you to sign in the documents with it. You should have thumbprint identity of this certificate (find it in the certificate information).

2. Then, simply run the app and follow the questions by putting your answers:

$> SamlIntegration.MetadataFileGenerator.exe

3. After file is successfully generated, it will appear in the same Applcation directory.

4. Provide metadata.xml and Public part of your certificate to the Service Provider.

## Saml Integration Utilities class library

A set of utility classes 

### How to use

1. Attach library or project to your project

2. Follow the Service Provider requirements and comment/remove the steps that are not necessary (i.e. sign of Assertion or SAMLResponse documents, encryption of Assertion document).

3. Call SamlIntegrationSteps->BuildEncodedSamlResponse(...) to create SAMLResponse document ready for sending via HTTP POST request.

## Follow

Ask questions using contacts shown here [Anton Yarkov](https://optiklab.github.io/)

## Copyright

Copyright © 2021 Anton Yarkov. All rights reserved.
Contacts: anton.yarkov@gmail.com

Licensed under the Apache License, Version 2.0 (the "License
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

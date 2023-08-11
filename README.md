# Semantic Search Research Project with ASP.NET Core

This Semantic Search Research Project represents an experimental endeavor to explore the possibilities of integrating natural language search capabilities within web applications. Leveraging Semantic Kernel with OpenAI large language models (LLMs), the project transforms common language queries into searchable parameters.

For example, a query like "I want a 3-bedroom house in Manchester for 250k" is translated into a structured search, providing a glimpse into a more intuitive and human-centric way to interact with applications: `api/properties?rooms=3&town=Manchester&price=250000`.

## Purpose and Scope

- **Research and Exploration:** This project is primarily aimed at researching the feasibility and effectiveness of using natural language in search queries.
- **Experimental Integration:** Experimenting with AI services like OpenAI to understand how natural language can be utilised in real-world scenarios.
- **Not for Production:** Please note that this project is experimental and not intended for production use. It serves as a proof-of-concept and a basis for further research and development.

## Getting Started

1. Generate an OpenAI API key. 
2. Add the key and your organisation ID to dotnet user secrets by running following commands within the `SemanticSearch.WebApi` directory:
```ps1
dotnet user-secrets set "OpenAiOptions:ApiKey" "{your key}"
dotnet user-secrets set "OpenAiOptions:OrganizationId" "{your org id}"
```
3. Build and run the project using standard ASP.NET Core commands or via something like Visual Studio:
```ps1
dotnet build && dotnet run
```

## Examples

![image](https://github.com/MickMelon/SemanticSearch/assets/21023513/64d1e0be-ebd9-4b4e-a68a-e6af8ac63611)
![image](https://github.com/MickMelon/SemanticSearch/assets/21023513/62cd5b90-0525-4f45-a3a1-98779859ba06)
![image](https://github.com/MickMelon/SemanticSearch/assets/21023513/685e9f3b-c882-4550-a514-e91899b5e930)


## Contributions

All contributions are welcome :) 

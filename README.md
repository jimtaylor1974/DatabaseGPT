# DatabaseGPT

DatabaseGTP is a console application that establishes a connection to a SQL SERVER database, enabling users to input natural language queries that are converted to SQL using OpenAI GPT API.

The concept behind this application was originally introduced in an article titled ["Replacing a SQL analyst with 26 recursive GPT prompts"](https://www.patterns.app/blog/2023/01/18/crunchbot-sql-analyst-gpt/).

To use this application you will need to set the OpenAiApiKey in the User Secrets file secrets.json

```json
{
  "AppSettings": {
    "OpenAiApiKey": "YOUR_KEY_HERE"
  }
}
```

Please keep in mind that this application is a proof of concept and does not have safeguards against prompt injection, malicious prompt engineering, user access control, or data mutation protection.

You can set the database connection in appsettings.json, the app will read the schema and use it to create the query prompt.

```json
{
  "AppSettings": {
    "ConnectionStringName": "OurBudget"
  },
  "ConnectionStrings": {
    "WorldWideImporters": "Data Source=.;Initial Catalog=WorldWideImporters;Integrated Security=True;TrustServerCertificate=True",
    "AdventureWorks2016": "Data Source=.;Initial Catalog=AdventureWorks2016;Integrated Security=True;TrustServerCertificate=True",
    "OurBudget": "Data Source=.;Initial Catalog=OurBudget;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

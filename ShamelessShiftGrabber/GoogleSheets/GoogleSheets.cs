using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace ShamelessShiftGrabber.GoogleSheets;

public class GoogleSheets
{
    public SheetsService Service { get; set; }

    private const string ApplicationName = "ShamelessGrabber";
    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    private readonly GoogleCredentialsConfiguration _googleConfiguration;

    public GoogleSheets(GoogleCredentialsConfiguration googleConfiguration)
    {
        _googleConfiguration = googleConfiguration;
        InitializeService();
    }

    private void InitializeService()
    {
        var credential = GetCredentialsFromFile();

        Service = new SheetsService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            }
        );
    }

    private GoogleCredential GetCredentialsFromFile()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_googleConfiguration);  
        var credential = GoogleCredential.FromJson(json).CreateScoped(Scopes);
        
        return credential;
    }
}
using System.Text.Json.Serialization;

namespace ShamelessShiftGrabber.GoogleSheets;

public class GoogleCredentialsConfiguration
{
    public GoogleCredentialsConfiguration(IConfiguration configuration)
    {
        configuration.Bind("GoogleCredentials", this);
    }

    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("project_id")]
    public string Project_id { get; set; }
    [JsonPropertyName("private_key_id")]
    public string Private_key_id { get; set; }
    [JsonPropertyName("private_key")]
    public string Private_key { get; set; }
    [JsonPropertyName("client_email")]
    public string Client_email { get; set; }
    [JsonPropertyName("client_id")]
    public string Client_id { get; set; }
    [JsonPropertyName("auth_uri")]
    public string Auth_uri { get; set; }
    [JsonPropertyName("token_uri")]
    public string Token_uri { get; set; }
    [JsonPropertyName("auth_provider_x509_cert_url")]
    public string Auth_provider_x509_cert_url { get; set; }
    [JsonPropertyName("client_x509_cert_url")]
    public string Client_x509_cert_url { get; set; }
    [JsonPropertyName("universe_domain")]
    public string Universe_domain { get; set; }
}
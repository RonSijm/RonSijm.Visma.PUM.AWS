using HtmlAgilityPack;

namespace RonSijm.VismaPUM.CLI.Features.SAML;

public static class GetSamlAssertionHandler
{
    public static async Task<string> GetSamlAssertion(string username, string password, string otp)
    {
        var session = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true
        });

        // 1st HTTP request: GET the login form
        var formResponse = await session.GetAsync(ProgramSettings.SamlAWSAssertionUrl);
        var formResponseContent = await formResponse.Content.ReadAsStringAsync();

        // Parse the login form
        var doc = new HtmlDocument();
        doc.LoadHtml(formResponseContent);
        var form = doc.DocumentNode.SelectSingleNode("//form");

        var payload = new Dictionary<string, string>();
        foreach (var input in form.SelectNodes("//input"))
        {
            var name = input.GetAttributeValue("name", "");
            var value = input.GetAttributeValue("value", "");

            if (string.IsNullOrEmpty(name)) continue;

            if (name.ToLower().Contains("username"))
            {
                payload[name] = username;
            }
            else if (name.ToLower().Contains("password"))
            {
                payload[name] = password;
            }
            else if (name.ToLower().Contains("authmethod"))
            {
                payload[name] = "FormsAuthentication";
            }
            else
            {
                payload[name] = value;
            }
        }

        var action = form.GetAttributeValue("action", ProgramSettings.SamlAWSAssertionUrl);
        var formResponseUrl = formResponse.RequestMessage.RequestUri.GetLeftPart(UriPartial.Authority) + action;

        // 2nd HTTP request: POST the login form
        var formContent = new FormUrlEncodedContent(payload);
        var response = await session.PostAsync(formResponseUrl, formContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Parse the challenge form
        doc.LoadHtml(responseContent);
        form = doc.DocumentNode.SelectSingleNode("//form");
        if (form == null)
        {
            throw new Exception("Challenge form not found.");
        }

        payload = new Dictionary<string, string>();
        foreach (var input in form.SelectNodes("//input"))
        {
            var name = input.GetAttributeValue("name", "");
            var value = input.GetAttributeValue("value", "");

            if (string.IsNullOrEmpty(name)) continue;

            if (name.ToLower().Contains("challenge"))
            {
                payload[name] = otp;
            }
            else if (name.ToLower().Contains("authmethod"))
            {
                payload[name] = "VismaMFAAdapter";
            }
            else
            {
                payload[name] = value;
            }
        }

        action = form.GetAttributeValue("action", response.RequestMessage.RequestUri.ToString());

        // 3rd HTTP request: POST the challenge form
        formContent = new FormUrlEncodedContent(payload);
        var tokenResponse = await session.PostAsync(action, formContent);
        var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

        // Extract the SAML assertion
        doc.LoadHtml(tokenResponseContent);
        var samlResponse = doc.DocumentNode
            .SelectSingleNode("//input[@name='SAMLResponse']")
            ?.GetAttributeValue("value", "");

        if (string.IsNullOrEmpty(samlResponse))
        {
            throw new Exception("Your login failed, please contact launch control or check token/username/password.");
        }

        return samlResponse;
    }
}
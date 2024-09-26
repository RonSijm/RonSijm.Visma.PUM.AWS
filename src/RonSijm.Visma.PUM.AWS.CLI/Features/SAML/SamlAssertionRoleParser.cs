using System.Xml.Linq;

namespace RonSijm.VismaPUM.CLI.Features.SAML;

public static class SamlAssertionRoleParser
{
    public static List<string> ParseRolesFromSamlAssertion(string samlAssertion)
    {
        var decodedSaml = Convert.FromBase64String(samlAssertion);
        var xml = XDocument.Parse(System.Text.Encoding.UTF8.GetString(decodedSaml));

        XNamespace saml2 = "urn:oasis:names:tc:SAML:2.0:assertion";

        var awsRoles = xml.Descendants(saml2 + "Attribute")
            .Where(attribute => attribute.Attribute("Name")?.Value == "https://aws.amazon.com/SAML/Attributes/Role")
            .SelectMany(attribute => attribute.Elements(saml2 + "AttributeValue"),
                (_, attributeValue) => attributeValue.Value).ToList();

        for (var i = 0; i < awsRoles.Count; i++)
        {
            var chunks = awsRoles[i].Split(',');
            if (chunks.Length == 2 && chunks[1].Contains("saml-provider"))
            {
                awsRoles[i] = $"{chunks[1]},{chunks[0]}";
            }
        }

        return awsRoles;
    }
}
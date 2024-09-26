namespace RonSijm.VismaPUM.AWSLib.Features.RoleSelection;

public static class AWSRoleSelector
{
    public static (string RoleArn, string PrincipalArn) SelectRole(List<string> awsRoles)
    {
        (string RoleArn, string PrincipalArn) roleArnAndPrincipalArn;

        switch (awsRoles.Count)
        {
            case > 1:
                roleArnAndPrincipalArn = SelectRole((IReadOnlyList<string>)awsRoles);
                break;
            case 1:
            {
                var parts = awsRoles[0].Split(',');
                roleArnAndPrincipalArn = (parts[1], parts[0]);
                break;
            }
            default:
                throw new Exception("No roles found, please contact launch control or check token/username/password.");
        }

        return roleArnAndPrincipalArn;
    }

    private static (string RoleArn, string PrincipalArn) SelectRole(IReadOnlyList<string> roles)
    {
        for (var i = 0; i < roles.Count; i++)
        {
            Console.WriteLine($"[{i}]: {roles[i]}");
        }

        Console.Write("Selection: ");
        var selectedIndex = int.Parse(Console.ReadLine());

        if (selectedIndex < 0 || selectedIndex >= roles.Count)
        {
            throw new Exception("You selected an invalid role index, please try again.");
        }

        var selectedRole = roles[selectedIndex];
        var parts = selectedRole.Split(',');

        return (RoleArn: parts[1], PrincipalArn: parts[0]);
    }
}
using Core.Config.Config.Model;

namespace Core.Config.Config;
public class ApiInformations
{
    public List<ConnectedProject> ConnectedProject { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
    //public JwtSettings JwtSettings { get; set; }
    public List<AllowedOrigins> AllowedOrigins { get; set; }
    public Redis Redis { get; set; }
}


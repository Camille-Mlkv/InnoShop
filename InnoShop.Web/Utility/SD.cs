namespace InnoShop.Web.Utility
{
    public class SD
    {
        public static string AuthAPIBase {  get; set; }
        public static string ProductAPIBase { get; set; }
        public const string TokenCookie = "JWTToken";
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
    }
}

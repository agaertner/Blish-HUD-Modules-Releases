using System.Collections.Specialized;

namespace Nekres.Notes.UI.Models
{
    internal class AuthResponseModel
    {
        public string Code { get; private init; }

        public string State { get; private init; }

        public string Error { get; private init; }

        public string ErrorDescription { get; private init; }

        public AuthResponseModel(){}

        public bool IsSuccess()
        {
            return !string.IsNullOrEmpty(this.Code) && !string.IsNullOrEmpty(this.State);
        }

        public bool IsError()
        {
            return !string.IsNullOrEmpty(this.Error);
        }

        public static AuthResponseModel FromQuery(NameValueCollection queryString)
        {
            return new AuthResponseModel
            {
                Code = queryString["code"],
                State = queryString["state"],
                Error = queryString["error"],
                ErrorDescription = queryString["error_description"]
            };
        }
    }
}

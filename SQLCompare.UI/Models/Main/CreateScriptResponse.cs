using SQLCompare.Core.Entities.AccountService;

namespace SQLCompare.UI.Models.Main
{
    /// <summary>
    /// Represent the response of a CreateScript request
    /// </summary>
    /// <seealso cref="SQLCompare.Core.Entities.AccountService.ABaseResponse" />
    public class CreateScriptResponse : ABaseResponse
    {
        /// <summary>
        /// Gets or sets the source SQL
        /// </summary>
        public string SourceSql { get; set; }

        /// <summary>
        /// Gets or sets the target SQL
        /// </summary>
        public string TargetSql { get; set; }
    }
}

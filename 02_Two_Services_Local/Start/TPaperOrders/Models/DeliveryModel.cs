namespace TPaperOrders
{
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.3.1.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class DeliveryModel
    {
        [Newtonsoft.Json.JsonProperty("id", Required = Newtonsoft.Json.Required.Always)]
        public int Id { get; set; }

        [Newtonsoft.Json.JsonProperty("number", Required = Newtonsoft.Json.Required.Always)]
        public double Number { get; set; }

        [Newtonsoft.Json.JsonProperty("clientId", Required = Newtonsoft.Json.Required.Always)]
        public int ClientId { get; set; }

        [Newtonsoft.Json.JsonProperty("ediOrderId", Required = Newtonsoft.Json.Required.Always)]
        public int EdiOrderId { get; set; }

        [Newtonsoft.Json.JsonProperty("productCode", Required = Newtonsoft.Json.Required.Always)]
        public int ProductCode { get; set; }

        [Newtonsoft.Json.JsonProperty("productId", Required = Newtonsoft.Json.Required.Always)]
        public int ProductId { get; set; }

        [Newtonsoft.Json.JsonProperty("notes", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Notes { get; set; }

        private System.Collections.Generic.IDictionary<string, object> _additionalProperties = new System.Collections.Generic.Dictionary<string, object>();

        [Newtonsoft.Json.JsonExtensionData]
        public System.Collections.Generic.IDictionary<string, object> AdditionalProperties
        {
            get { return _additionalProperties; }
            set { _additionalProperties = value; }
        }
    }
}
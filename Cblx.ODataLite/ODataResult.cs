using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cblx.ODataLite
{
    public class ODataResult
    {
        readonly IODataParameters oDataParameters;
        readonly Type itemType;
        protected IEnumerable value;

        public int? Count { get; set; }

        public ODataResult(IEnumerable items, int? count, IODataParameters oDataParameters, Type itemType)
        {
            this.itemType = itemType;
            value = items;
            Count = count;
            this.oDataParameters = oDataParameters;
        }

        public List<Dictionary<string, object>> ConvertToRestrictedValue()
        {
            //Propriedades selecionadas
            var selected = oDataParameters.Select.Split(",");

            //Obtenho as props
            var lstProps = selected.Select(
                s =>
                {
                    var prop = itemType.GetProperties().FirstOrDefault(prop => string.Equals(s, prop.Name, StringComparison.OrdinalIgnoreCase));
                    if (prop == null) { throw new ArgumentException($"Property {s} not found!"); }
                    return prop;
                }
            ).ToList();

            //Crio a lista de resultados
            var result = new List<Dictionary<string, object>>();

            foreach (var item in value)
            {
                var dct = new Dictionary<string, object>();
                result.Add(dct);
                foreach (var p in lstProps)
                {
                    dct[System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(p.Name)] = p.GetValue(item);
                }
            }
            return result;
        }
    }


    [JsonConverter(typeof(ODataResultConverter))]
    public class ODataResult<TItem> : ODataResult
    {
        //Adicionado construtor padrão para permitir desserialziaçãio
        public ODataResult() : base(null, null, null, typeof(TItem)) { }

        public ODataResult(List<TItem> items, int? count, IODataParameters oDataParameters) : base(items, count, oDataParameters, typeof(TItem)) { }

        public IEnumerable<TItem> Value { 
            get { return this.value as IEnumerable<TItem>; }
            set { this.value = value; }

        }
    }

    public class ODataResultConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ODataResult<>);
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //No need to implement
            return null;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            var oDataResult = value as ODataResult;

            writer.WriteStartObject();
            if (oDataResult.Count.HasValue)
            {
                writer.WriteNumber("count", oDataResult.Count.Value);
                writer.WriteNumber("@odata.count", oDataResult.Count.Value);
            }
            writer.WriteStartArray("value");
            foreach(var item in oDataResult.ConvertToRestrictedValue())
            {
                JsonSerializer.Serialize(writer, item, options);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}

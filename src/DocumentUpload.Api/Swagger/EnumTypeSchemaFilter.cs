using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentUpload.Api.Swagger
{
	internal sealed class EnumTypeSchemaFilter : ISchemaFilter
	{
		private const string ExtensionEnums = "x-ms-enum",
			ExtensionValuesProperty = "values",
			ExtensionAsStringProperty = "modelAsString";

		private readonly bool _useInts;

		/// <param name="useInts">if set to <c>true</c> enums are described as integers instead of strings in the swagger website and schema.</param>
		[UsedImplicitly]
		public EnumTypeSchemaFilter(bool useInts = false) => _useInts = useInts;

		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			var type = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
            if (!type.IsEnum) 
                return;

            schema.Format = SwaggerConstants.FormatInt32;
            schema.Type = SwaggerConstants.TypeString;

            var schemaValues = new List<IOpenApiAny>();

            var values = Enum.GetValues(type).Cast<object>().Select(Convert.ToInt32).ToList();
            var names = Enum.GetNames(type);

            if (_useInts)
            {
                foreach (var e in values)
                    schemaValues.Add(new OpenApiInteger(e));
            }
            else
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var e in names)
                    schemaValues.Add(new OpenApiString(e));
            }

            schema.Enum = schemaValues;


            var valueArray = new OpenApiArray();

            for (var i = 0; i < names.Length; ++i)
            {
                var val = values[i];

                var o = new OpenApiObject
                {
                    [SwaggerConstants.ValueProperty] = _useInts
                            ? (IOpenApiAny) new OpenApiInteger(Convert.ToInt32(val))
                            : new OpenApiString(names[i]),

                    [SwaggerConstants.NameProperty] = new OpenApiString(names[i])
                };

                valueArray.Add(o);

            }

            schema.AddExtension(ExtensionEnums, new OpenApiObject
            {
                [SwaggerConstants.NameProperty] = new OpenApiString(type.Name),
                [ExtensionAsStringProperty] = new OpenApiBoolean(false),
                [ExtensionValuesProperty] = valueArray
            });

        }
	}
}
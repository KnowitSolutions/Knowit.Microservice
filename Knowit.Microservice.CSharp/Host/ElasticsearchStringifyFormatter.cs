using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;

namespace Host
{
	public class ElasticsearchStringifyFormatter : ExceptionAsObjectJsonFormatter
	{
		public ElasticsearchStringifyFormatter() : base(false, string.Empty, true, null, null, true)
		{
		}

		protected override void WriteProperties(
			IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output) =>
			base.WriteProperties(Stringify(properties), output);

		private static IReadOnlyDictionary<string, LogEventPropertyValue> Stringify(
			IReadOnlyDictionary<string, LogEventPropertyValue> properties) =>
			properties.ToDictionary(entry => entry.Key, entry => Stringify(entry.Value));

		private static LogEventPropertyValue Stringify(LogEventPropertyValue value)
		{
			switch (value)
			{
				case ScalarValue scalar:
					var element = scalar.Value.ToString();
					return new ScalarValue(element);

				case SequenceValue sequence:
					var elements = sequence.Elements.Select(Stringify);
					return new SequenceValue(elements);

				case DictionaryValue dictionary:
					var entries = dictionary.Elements.Select(entry =>
						new KeyValuePair<ScalarValue, LogEventPropertyValue>(
							(ScalarValue) Stringify(entry.Key), Stringify(entry.Value)));
					return new DictionaryValue(entries);

				case StructureValue structure:
					var properties = structure.Properties.Select(property =>
						new LogEventProperty(property.Name, Stringify(property.Value)));
					return new StructureValue(properties);

				default:
					throw new ArgumentException("Invalid property type");
			}
		}
	}
}

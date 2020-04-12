using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using DocumentUpload.Core;


namespace DocumentUpload.Services.Generators
{
	public class TextDescriptionGenerator : DescriptionGeneratorBase
	{
		public const int DefaultMaxLength = 20;

		private readonly int _maxDescriptionLen;

		public TextDescriptionGenerator(int maxDescriptionLen = DefaultMaxLength)
		{
			_maxDescriptionLen = maxDescriptionLen;
		}

		public override DocumentType Type => DocumentType.Text;

		protected override ValueTask<string> GenerateDescription(ReadOnlyMemory<byte> fileContent)
		{
			
            var text = GetLeadingText(fileContent, _maxDescriptionLen);

            var output = string.IsNullOrWhiteSpace(text)
                    ? "Empty Text File"
                    : $"Text file beginning with '{text}'";

            return new ValueTask<string>(output);
		}

        internal static string GetLeadingText(ReadOnlyMemory<byte> content, int maxRead = DefaultMaxLength)
        {
            if (maxRead <= 0)
                maxRead = DefaultMaxLength;

            var decoder = Encoding.UTF8.GetDecoder();
            var buffer = ArrayPool<char>.Shared.Rent(maxRead);

            try
            {

                var pos = 0;
                bool completed = false;
                while (pos < content.Length && !completed)
                {
                    decoder.Convert(content.Span.Slice(pos), buffer, true, out int bytesUsed, out int charsUsed, out completed);

                    var temp = Trim(buffer.AsSpan(0, charsUsed));
                    var idx = temp.IndexOfAny('\r', '\n');
                    if (idx > 0)
                        temp = temp.Slice(0, idx);

                    // need to limit how much we take back
                    if (temp.Length > maxRead)
                        temp = temp.Slice(0, maxRead);

                    temp = Trim(temp);

                    if (!temp.IsEmpty)
                        return temp.ToString();


                    pos += bytesUsed;
                }

            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }

            return null;

        }


		// not available in NETStandard ... primitive implementation
	    internal static Span<char> Trim(Span<char> span)
        {
            if (span.IsEmpty)
                return span;

            int start = 0, end = span.Length - 1;
            char startChar = span[start], endChar = span[end];

            while (start < end && (char.IsWhiteSpace(startChar) || char.IsWhiteSpace(endChar)))
            {
                if (char.IsWhiteSpace(startChar))
                    start++;

                if (char.IsWhiteSpace(endChar))
                    end--;

                startChar = span[start];
                endChar = span[end];
            }

            return span.Slice(start, end - start + 1);
        }
	}
}
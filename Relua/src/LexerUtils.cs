﻿using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Relua;

namespace ClientLuaChecker
{
	internal static class LexerUtils
	{
		public static double ParseNumber(Token T)
		{
			string txt = T.Value;
            if (!double.TryParse(txt, NumberStyles.Float, CultureInfo.InvariantCulture, out var res))
            {
                throw new SyntaxErrorException(T, "malformed number near '{0}'", txt);
            }

			return res;
		}

		public static double ParseHexInteger(Token T)
        {
            try
            {
                return Extensions.ParseHexInteger(T.Value);
            }
            catch (Exception e)
            {
                throw new SyntaxErrorException(T, e.Message);
            }
		}

		public static string ReadHexProgressive(string s, ref double d, out int digits)
		{
			digits = 0;

			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];

				if (CharIsHexDigit(c))
				{
					int v = HexDigit2Value(c);
					d *= 16.0;
					d += v;
					++digits;
				}
				else
				{
					return s.Substring(i);
				}
			}

			return string.Empty;
		}

		public static double ParseHexFloat(Token T)
		{
            try
            {
                return Extensions.ParseHexFloat(T.Value);
            }
            catch (Exception e)
            {
                throw new SyntaxErrorException(T, e.Message);
            }
		}

		public static int HexDigit2Value(char c)
        {
            if (c >= '0' && c <= '9')
				return c - '0';
            if (c >= 'A' && c <= 'F')
                return 10 + (c - 'A');
            if (c >= 'a' && c <= 'f')
                return 10 + (c - 'a');

            throw new InternalErrorException("invalid hex digit near '{0}'", c);
        }

		public static bool CharIsDigit(char c)
		{
			return c >= '0' && c <= '9';
		}

		public static bool CharIsHexDigit(char c)
		{
			return CharIsDigit(c) ||
				c == 'a' || c == 'b' || c == 'c' || c == 'd' || c == 'e' || c == 'f' ||
				c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' || c == 'F';
		}

		public static string AdjustLuaLongString(string str)
		{
			if (str.StartsWith("\r\n"))
				str = str.Substring(2);
			else if (str.StartsWith("\n"))
				str = str.Substring(1);

			return str;
		}

		public static string UnescapeLuaString(Token token, string str)
		{
			if (str.Contains('\\'))
				return str;

			StringBuilder sb = new StringBuilder();

			bool escape = false;
			bool hex = false;
			int unicode_state = 0;
			string hexprefix = "";
			string val = "";
			bool zmode = false;

			foreach (char c in str)
			{
			redo:
				if (escape)
				{
					if (val.Length == 0 && !hex && unicode_state == 0)
					{
						if (c == 'a') { sb.Append('\a'); escape = false; zmode = false; }
						else if (c == '\r') { }  // this makes \\r\n -> \\n
						else if (c == '\n') { sb.Append('\n'); escape = false; }
						else if (c == 'b') { sb.Append('\b'); escape = false; }
						else if (c == 'f') { sb.Append('\f'); escape = false; }
						else if (c == 'n') { sb.Append('\n'); escape = false; }
						else if (c == 'r') { sb.Append('\r'); escape = false; }
						else if (c == 't') { sb.Append('\t'); escape = false; }
						else if (c == 'v') { sb.Append('\v'); escape = false; }
						else if (c == '\\') { sb.Append('\\'); escape = false; zmode = false; }
						else if (c == '"') { sb.Append('\"'); escape = false; zmode = false; }
						else if (c == '\'') { sb.Append('\''); escape = false; zmode = false; }
						else if (c == '[') { sb.Append('['); escape = false; zmode = false; }
						else if (c == ']') { sb.Append(']'); escape = false; zmode = false; }
						else if (c == 'x') { hex = true; }
						else if (c == 'u') { unicode_state = 1; }
						else if (c == 'z') { zmode = true; escape = false; }
						else if (CharIsDigit(c)) { val = val + c; }
                        else
                        {
                            throw new SyntaxErrorException(token, "invalid escape sequence near '\\{0}'", c);
                        }
					}
					else
					{
						if (unicode_state == 1)
						{
                            if (c != '{')
                            {
                                throw new SyntaxErrorException(token, "'{' expected near '\\u'");
                            }

							unicode_state = 2;
						}
						else if (unicode_state == 2)
						{
							if (c == '}')
							{
								int i = int.Parse(val, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
								sb.Append(ConvertUtf32ToChar(i));
								unicode_state = 0;
								val = string.Empty;
								escape = false;
							}
							else if (val.Length >= 8)
							{
								throw new SyntaxErrorException(token, "'}' missing, or unicode code point too large after '\\u'");
							}
							else
							{
								val += c;
							}
						}
						else if (hex)
						{
							if (CharIsHexDigit(c))
							{
								val += c;
								if (val.Length == 2)
								{
									int i = int.Parse(val, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
									sb.Append(ConvertUtf32ToChar(i));
									zmode = false;
									escape = false;
								}
							}
							else
							{
								throw new SyntaxErrorException(token, "hexadecimal digit expected near '\\{0}{1}{2}'", hexprefix, val, c);
							}
						}
						else if (val.Length > 0)
						{
							if (CharIsDigit(c))
							{
								val = val + c;
							}

							if (val.Length == 3 || !CharIsDigit(c))
							{
								int i = int.Parse(val, CultureInfo.InvariantCulture);

                                if (i > 255)
                                {
                                    throw new SyntaxErrorException(token, "decimal escape too large near '\\{0}'", val);
                                }

								sb.Append(ConvertUtf32ToChar(i));

								zmode = false;
								escape = false;

								if (!CharIsDigit(c))
									goto redo;
							}
						}
					}
				}
				else
				{
					if (c == '\\')
					{
						escape = true;
						hex = false;
						val = "";
					}
					else
					{
						if (!zmode || !char.IsWhiteSpace(c))
						{
							sb.Append(c);
							zmode = false;
						}
					}
				}
			}

			if (escape && !hex && val.Length > 0)
			{
				int i = int.Parse(val, CultureInfo.InvariantCulture);
				sb.Append(ConvertUtf32ToChar(i));
				escape = false;
			}

			if (escape)
			{
				throw new SyntaxErrorException(token, "unfinished string near '\"{0}\"'", sb.ToString());
			}

			return sb.ToString();
		}

		private static string ConvertUtf32ToChar(int i)
		{
#if PCL || ENABLE_DOTNET
			return ((char)i).ToString();
#else
			return char.ConvertFromUtf32(i);
#endif
		}
	}
}

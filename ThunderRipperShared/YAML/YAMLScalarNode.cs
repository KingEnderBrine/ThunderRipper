using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ThunderRipperShared.YAML.Extensions;

namespace ThunderRipperShared.YAML
{
    public sealed class YAMLScalarNode : YAMLNode
    {
        public static YAMLScalarNode Empty { get; } = new YAMLScalarNode();

        public override YAMLNodeType NodeType => YAMLNodeType.Scalar;
        public override bool IsMultiline => false;
        public override bool IsIndent => false;

        public ScalarStyle Style { get; }

        private ScalarUnion m_value = default;
        public ScalarUnion Value => m_value;

        public string StringValue => Style == ScalarStyle.Hex ? m_value.ToHexString() : m_value.ToString();

        private static readonly Regex s_illegal = new Regex("(^\\s)|(^-\\s)|(^-$)|(^[\\:\\[\\]'\"*&!@#%{}?<>,\\`])|([:@]\\s)|([\\n\\r])|([:\\s]$)", RegexOptions.Compiled);


        public YAMLScalarNode()
        {
        }

        public YAMLScalarNode(bool value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(bool value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(char value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(char value, bool _)
        {
            SetValue(value);
        }

        public YAMLScalarNode(sbyte value) :
           this(value, false)
        {
        }

        public YAMLScalarNode(sbyte value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(byte value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(byte value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(short value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(short value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(ushort value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(ushort value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(int value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(int value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(uint value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(uint value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(long value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(long value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(ulong value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(ulong value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(float value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(float value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(double value) :
            this(value, false)
        {
        }

        public YAMLScalarNode(double value, bool isHex)
        {
            SetValue(value);
            Style = isHex ? ScalarStyle.Hex : ScalarStyle.Plain;
        }

        public YAMLScalarNode(string value)
        {
            SetValue(value);
            Style = GetStringStyle(value);
        }

        internal YAMLScalarNode(string value, bool _)
        {
            SetValue(value);
            Style = ScalarStyle.Plain;
        }

        public void SetValue(bool value) => m_value.Set(value);
        public void SetValue(char value) => m_value.Set(value);
        public void SetValue(byte value) => m_value.Set(value);
        public void SetValue(sbyte value) => m_value.Set(value);
        public void SetValue(short value) => m_value.Set(value);
        public void SetValue(ushort value) => m_value.Set(value);
        public void SetValue(int value) => m_value.Set(value);
        public void SetValue(uint value) => m_value.Set(value);
        public void SetValue(long value) => m_value.Set(value);
        public void SetValue(ulong value) => m_value.Set(value);
        public void SetValue(float value) => m_value.Set(value);
        public void SetValue(double value) => m_value.Set(value);
        public void SetValue(string value) => m_value.Set(value);

        internal override void Emit(Emitter emitter)
        {
            base.Emit(emitter);

            switch (Style)
            {
                case ScalarStyle.Hex:
                case ScalarStyle.Plain:
                    m_value.WriteTo(emitter, Style);
                    break;

                case ScalarStyle.SingleQuoted:
                    emitter.Write('\'');
                    m_value.WriteTo(emitter, Style);
                    emitter.Write('\'');
                    break;

                case ScalarStyle.DoubleQuoted:
                    emitter.Write('"');
                    m_value.WriteTo(emitter, Style);
                    emitter.Write('"');
                    break;

                default:
                    throw new Exception($"Unsupported scalar style {Style}");
            }
        }

        private static ScalarStyle GetStringStyle(string value)
        {
            if (s_illegal.IsMatch(value))
            {
                return value.Contains("\n ") ? ScalarStyle.DoubleQuoted : ScalarStyle.SingleQuoted;
            }
            return ScalarStyle.Plain;
        }
    }
}

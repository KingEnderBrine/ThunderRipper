using System;
using System.Collections.Generic;
using System.Text;
using ThunderRipperShared.YAML;

namespace ThunderRipperShared.Assets
{
    public interface IYAMLExportable
    {
        YAMLNode ExportYAML();
    }
}

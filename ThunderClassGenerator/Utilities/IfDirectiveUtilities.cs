using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRipperShared.Utilities;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ThunderClassGenerator.Utilities
{
    public static class IfDirectiveUtilities
    {
        public static UnityVersionRange[] GetDirectiveVersions(IfDirectiveTriviaSyntax ifDirective)
        {
            var ranges = new List<UnityVersionRange>();
            var current = ifDirective.Condition;
            while (current is BinaryExpressionSyntax binaryExpression)
            {
                if (binaryExpression.Right is ParenthesizedExpressionSyntax parenthesizedExpression)
                {
                    var versionsExpression = parenthesizedExpression.Expression as BinaryExpressionSyntax;
                    var minVersion = GetVersionFromIdentifier(versionsExpression.Left as IdentifierNameSyntax);
                    var maxVersion = GetVersionFromIdentifier((versionsExpression.Right as PrefixUnaryExpressionSyntax).Operand as IdentifierNameSyntax);
                    ranges.Add(new UnityVersionRange(minVersion, maxVersion));
                }
                else if (binaryExpression.Right is IdentifierNameSyntax identifier)
                {
                    ranges.Add(new UnityVersionRange(GetVersionFromIdentifier(identifier), default));
                }
                else if (binaryExpression.Right is PrefixUnaryExpressionSyntax prefixUnary)
                {
                    ranges.Add(new UnityVersionRange(default, GetVersionFromIdentifier(prefixUnary.Operand as IdentifierNameSyntax)));
                }
                current = binaryExpression.Left;
            }

            return ranges.OrderBy(r => r).ToArray();
        }

        public static UnityVersionRange[] RecalculateRanges(UnityVersionRange[] ranges, UnityVersion version, IEnumerable<UnityVersion> supportedVersions, bool existsInVersion, bool isNew)
        {
            //If this is the first supportedVersion then there's no need for restrictions
            if (!supportedVersions.Any())
            {
                return Array.Empty<UnityVersionRange>();
            }

            //Expect supportedVersions to be in ascending order
            var previousSupportedVersion = supportedVersions.LastOrDefault(v => v < version);
            var nextSupportedVersion = supportedVersions.FirstOrDefault(v => v > version);

            if (existsInVersion)
            {
                return UpdateRangesWithExisting(ranges, version, previousSupportedVersion, nextSupportedVersion, isNew);
            }
            
            return UpdateRangesWithNotExisting(ranges, version, previousSupportedVersion, nextSupportedVersion);
        }

        private static UnityVersionRange[] UpdateRangesWithNotExisting(UnityVersionRange[] ranges, UnityVersion version, UnityVersion previousSupportedVersion, UnityVersion nextSupportedVersion)
        {
            if (ranges.Length == 0)
            {
                //If there is no nextSupportedVersion this means element is expected to exist before this version, but not past it
                if (nextSupportedVersion == default)
                {
                    return new UnityVersionRange[] { new UnityVersionRange(default, version) };
                }

                //If there is no previousSupportedVersion this means element is expected to exist starting from nextSupportedVersion
                if (previousSupportedVersion == default)
                {
                    return new UnityVersionRange[] { new UnityVersionRange(nextSupportedVersion, default) };
                }

                //If there is nextSupportedVersion this means element is expected to exist before this version, and starting from nextSupportedVersion
                return new UnityVersionRange[] { new UnityVersionRange(default, version), new UnityVersionRange(nextSupportedVersion, default) };
            }

            var result = new List<UnityVersionRange>();
            var rangeUpdated = false;
            //Expect ranges to be in ascending order and without intersections
            foreach (var range in ranges)
            {
                //if range was updated, can just add all remaining ranges without any checks
                if (rangeUpdated)
                {
                    result.Add(range);
                    continue;
                }

                //If version is bigger than range this means it should be processed later
                if (range.HasMax && version > range.max)
                {
                    result.Add(range);
                    continue;
                }

                if (range.Contains(version))
                {
                    if (previousSupportedVersion != default)
                    {
                        result.Add(new UnityVersionRange(range.min, version));
                    }

                    if (nextSupportedVersion != default && nextSupportedVersion != range.max)
                    {
                        result.Add(new UnityVersionRange(nextSupportedVersion, range.max));
                    }

                    rangeUpdated = true;
                    continue;
                }

                result.Add(range);
                rangeUpdated = true;
            }

            if (!rangeUpdated)
            {
                if (result.LastOrDefault() == default)
                {
                    result.Add(new UnityVersionRange(default, version));
                }
            }

            return result.ToArray();
        }

        private static UnityVersionRange[] UpdateRangesWithExisting(UnityVersionRange[] ranges, UnityVersion version, UnityVersion previousSupportedVersion, UnityVersion nextSupportedVersion, bool isNew)
        {
            if (ranges.Length == 0)
            {
                //If it's a new element this means it's only supported between version and nextSupportedVersion
                if (isNew)
                {
                    if (previousSupportedVersion == default)
                    {
                        return new UnityVersionRange[] { new UnityVersionRange(default, nextSupportedVersion) };
                    }

                    return new UnityVersionRange[] { new UnityVersionRange(version, nextSupportedVersion) };
                }

                //If there are no ranges and element already exists this means it's already supported in all versions, thus no need for restrictions
                return Array.Empty<UnityVersionRange>();
            }

            var result = new List<UnityVersionRange>();
            var rangeUpdated = false;
            //Expect ranges to be in ascending order and without intersections
            foreach (var range in ranges)
            {
                //if range was updated, can just add all remaining ranges without any checks
                if (rangeUpdated)
                {
                    result.Add(range);
                    continue;
                }

                //If version is bigger than range this means it should be processed later
                if (range.HasMax && version > range.max)
                {
                    result.Add(range);
                    continue;
                }

                //If range contains version it means element is already supported
                if (range.Contains(version))
                {
                    result.Add(range);
                    rangeUpdated = true;
                    continue;
                }

                //If there is no supported version between version and range the range can be expanded
                if (nextSupportedVersion == range.min)
                {
                    result.Add(new UnityVersionRange(version, range.max));
                }
                else
                {
                    result.Add(new UnityVersionRange(version, nextSupportedVersion));
                    result.Add(range);
                }
                rangeUpdated = true;
            }

            if (!rangeUpdated)
            {
                result.Add(new UnityVersionRange(version, nextSupportedVersion));
            }

            return result.ToArray();
        }

        public static IfDirectiveTriviaSyntax GetIfDirectiveFromVersionRanges(UnityVersionRange[] ranges)
        {
            ExpressionSyntax condition = SF.IdentifierName("CG");
            foreach (var range in ranges.OrderBy(r => r))
            {
                condition = SF.BinaryExpression(SyntaxKind.LogicalOrExpression, condition, GetNodeForVersionRange(range));
            }

            return SF.IfDirectiveTrivia(condition, true, true, true);
        }

        private static ExpressionSyntax GetNodeForVersionRange(UnityVersionRange range)
        {
            var min = range.HasMin ? SF.IdentifierName(range.min.ToDirectiveString()) : null;
            var max = range.HasMax ? SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SF.IdentifierName(range.max.ToDirectiveString())) : null;

            if (min != null && max != null)
            {
                return SF.ParenthesizedExpression(SF.BinaryExpression(SyntaxKind.LogicalAndExpression, min, max));
            }

            if (min != null)
            {
                return min;
            }

            if (max != null)
            {
                return max;
            }

            throw new NotSupportedException($"\"{nameof(range)}\" can't be default");
        }

        private static UnityVersion GetVersionFromIdentifier(IdentifierNameSyntax identifier)
        {
            return new UnityVersion(identifier.Identifier.ValueText);
        }
    }
}

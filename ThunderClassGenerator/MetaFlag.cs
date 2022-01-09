using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator
{
    enum MetaFlag
    {
		NoTransferFlags = 0,
		// Putting this mask in a transfer will make the variable be hidden in the property editor
		HideInEditorMask = 1 << 0,

		// Makes a variable not editable in the property editor
		NotEditableMask = 1 << 4,

		// There are 3 types of PPtrs: kStrongPPtrMask, default (weak pointer)
		// a Strong PPtr forces the referenced object to be cloned.
		// A Weak PPtr doesnt clone the referenced object, but if the referenced object is being cloned anyway (eg. If another (strong) pptr references this object)
		// this PPtr will be remapped to the cloned object
		// If an  object  referenced by a WeakPPtr is not cloned, it will stay the same when duplicating and cloning, but be NULLed when templating
		StrongPPtrMask = 1 << 6,
		// unused  = 1 << 7,

		// kEditorDisplaysCheckBoxMask makes an integer variable appear as a checkbox in the editor
		EditorDisplaysCheckBoxMask = 1 << 8,

		// unused = 1 << 9,
		// unused = 1 << 10,

		// Show in simplified editor
		SimpleEditorMask = 1 << 11,

		// When the options of a serializer tells you to serialize debug properties kSerializeDebugProperties
		// All debug properties have to be marked kDebugPropertyMask
		// Debug properties are shown in expert mode in the inspector but are not serialized normally
		DebugPropertyMask = 1 << 12,

		AlignBytesFlag = 1 << 14,
		AnyChildUsesAlignBytesFlag = 1 << 15,
		IgnoreWithInspectorUndoMask = 1 << 16,

		// unused = 1 << 18,

		// Ignore this property when reading or writing .meta files
		IgnoreInMetaFiles = 1 << 19,

		// When reading meta files and this property is not present, read array entry name instead (for backwards compatibility).
		TransferAsArrayEntryNameInMetaFiles = 1 << 20,

		// When writing YAML Files, uses the flow mapping style (all properties in one line, with "{}").
		TransferUsingFlowMappingStyle = 1 << 21,

		// Tells SerializedProperty to generate bitwise difference information for this field.
		GenerateBitwiseDifferences = 1 << 22,

		DontAnimate = 1 << 23,
	}
}

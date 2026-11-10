@tool
extends EditorPlugin
var exporter: EditorExportPlugin
func _enter_tree():
	exporter = HapticsExport.new()
	add_export_plugin(exporter)
func _exit_tree():
	remove_export_plugin(exporter)
class HapticsExport extends EditorExportPlugin:
	func _get_name() -> String:
		return "BubbleHaptics"
	func _supports_platform(platform: EditorExportPlatform) -> bool:
		return platform is EditorExportPlatformAndroid
	func _get_android_libraries(_platform: EditorExportPlatform, _debug: bool) -> PackedStringArray:
		return PackedStringArray(["res://addons/bubble_haptics/bin/BubbleHaptics.aar"])

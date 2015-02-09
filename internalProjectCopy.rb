# Copies and standardizes certain folders with strike
base_dir = Dir.pwd
$build_dir = build_dir = "C:/Users/Jon/Documents/Github/strike"
require 'FileUtils'
Dir.chdir Dir.pwd + "/Assets/Editor Default Resources"
vals = Dir.glob("*.prefab")
for v in vals do
  v = Dir.pwd + v
end
FileUtils.cp_r(vals, $build_dir + "/Assets/Resources")
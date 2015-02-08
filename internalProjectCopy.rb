# Copies and standardizes certain folders with strike
base_dir = Dir.pwd
$build_dir = build_dir = "C:/Users/Jon/Documents/Github/strike"
require 'FileUtils'

FileUtils.cp_r(Dir.pwd + "/Assets/Editor Default Resources/.", Dir.pwd + "/Assets/Resources")
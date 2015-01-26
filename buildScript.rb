print "Build name?\n> "
build_name = $stdin.gets.chomp
base_dir = Dir.pwd
$build_dir = build_dir = "C:/Users/Jon/Documents/strikeBuild/builds/#{build_name}"
remote_build_dir = "C:/Users/Jon/Dropbox/Public/strike"
web = "-buildWebPlayer #{remote_build_dir}"
win32 = "-buildWindowsPlayer #{build_dir}/gridia-#{build_name}-win32/client.exe"
win64 = "-buildWindows64Player #{build_dir}/gridia-#{build_name}-win64/client.exe"
osx = "-buildOSXPlayer #{build_dir}/gridia-#{build_name}-osx/client.app"
linux32 = "-buildLinux32Player #{build_dir}/gridia-#{build_name}-linux32/client.app"
linux64 = "-buildLinux64Player #{build_dir}/gridia-#{build_name}-linux64/client.app"
puts 'building clients for each platform'
#add web functionality back #{web}
unity = '"%PROGRAMFILES%/Unity/Editor/Unity.exe"'
`#{unity} -batchmode -quit -projectPath #{base_dir} #{win32} #{win64} #{osx} #{linux32} #{linux64}`
require 'FileUtils'

FileUtils.cp_r(Dir.pwd + "/Assets/Data/.", "#{build_dir}/gridia-#{build_name}-win32/client_Data/Data")
FileUtils.cp_r(Dir.pwd + "/Assets/Maps/.", "#{build_dir}/gridia-#{build_name}-win32/client_Data/Maps")

FileUtils.cp_r(Dir.pwd + "/Assets/Data/.", "#{build_dir}/gridia-#{build_name}-win64/client_Data/Data")
FileUtils.cp_r(Dir.pwd + "/Assets/Maps/.", "#{build_dir}/gridia-#{build_name}-win64/client_Data/Maps")

FileUtils.cp_r(Dir.pwd + "/Assets/Data/.", "#{build_dir}/gridia-#{build_name}-osx/client.app/Contents/Data/Data")
FileUtils.cp_r(Dir.pwd + "/Assets/Maps/.", "#{build_dir}/gridia-#{build_name}-osx/client.app/Contents/Data/Data")

FileUtils.cp_r(Dir.pwd + "/Assets/Data/.", "#{build_dir}/gridia-#{build_name}-linux32/client_Data/Data")
FileUtils.cp_r(Dir.pwd + "/Assets/Maps/.", "#{build_dir}/gridia-#{build_name}-linux32/client_Data/Maps")

FileUtils.cp_r(Dir.pwd + "/Assets/Data/.", "#{build_dir}/gridia-#{build_name}-linux64/client_Data/Data")
FileUtils.cp_r(Dir.pwd + "/Assets/Maps/.", "#{build_dir}/gridia-#{build_name}-linux64/client_Data/Maps")

FileUtils.cp_r(Dir.pwd + "/Assets/Data/.", "#{remote_build_dir}/Data")
FileUtils.cp_r(Dir.pwd + "/Assets/Maps/.", "#{remote_build_dir}/Maps")
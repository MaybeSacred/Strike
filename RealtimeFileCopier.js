var fs = require("fs");
var stringies = require("string");
var _ = require("underscore");
var projectPath = process.argv[2];///strike/Assets";
var totalLines = 0;
var extension = process.argv.slice(3, process.argv.length) || [""];
var walk = function(dir, done) {
  var results = [];
  fs.readdir(dir, function(err, list) {
    if (err) return done(err);
    var pending = list.length;
    if (!pending) return done(null, results);
    list.forEach(function(file) {
      file = dir + '/' + file;
      fs.stat(file, function(err, stat) {
        if (stat && stat.isDirectory()) {
          walk(file, function(err, res) {
            results = results.concat(res);
            if (!--pending) done(null, results);
          });
        } else {
          results.push(file);
          if (!--pending) done(null, results);
        }
      });
    });
  });
};
function done(err, files){
  _.each(files, function(element, index, list){
    _.each(extension, function(strElement, strIndex, strList){
      if(stringies(element).endsWith(strElement)){
        totalLines += CountLinesInFile(element);
      }
    });
  });
  console.log(totalLines);
}
function CountLinesInFile(fileName){
  var lines = 0;
  var fileData = fs.readFileSync(fileName);
  //, 'utf8', function(err, fileData){
  fileData = fileData.toString().split("\n");
  lines += fileData.length;
  return lines;
}
walk(projectPath, done);

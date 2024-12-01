find . -iname "bin" | xargs rm -rf
find . -iname "obj" | xargs rm -rf
find . -iname ".idea" | xargs rm -rf

find . -type d -empty -not -path "./.git/*" | xargs rm -rf
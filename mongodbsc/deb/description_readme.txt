Normally the extended description of a Debian package has each line
begin with a leading space for lines that are part of a paragraph and that
will be word-wrapped when displayed or two leading spaces for lines that
will be displayed verbatim. fpm adds two leading spaces to the extended
description lines even if the line already has a leading space. So
the extended description in description.txt does not include any leading
spaces.
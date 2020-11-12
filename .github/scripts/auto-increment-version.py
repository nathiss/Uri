#!/usr/bin/env python3

import xml.etree.ElementTree as ET

CSPROJ_FILE = 'Uri/Uri.csproj'
ENV_VARIABLE_FILE = '.env'
ENV_VARIABLE_NAME = 'URI_VERSION'

csproj_tree = ET.parse(CSPROJ_FILE)
version_tag = csproj_tree.getroot().find('PropertyGroup').find('Version')

version_parts = version_tag.text.split('.', 2)
version_parts[-1] = str(int(version_parts[-1]) + 1)
new_version = '.'.join(version_parts)

with open(ENV_VARIABLE_FILE, 'w') as f:
    f.write(ENV_VARIABLE_NAME + '=' + new_version + '\n')

version_tag.text = new_version
csproj_tree.write(CSPROJ_FILE)

# Debian Package Builder

"debbuild" is a small Windows tool and C# library to create .deb (or compatible) packages. If you don't want to install cygwin or otherwise mess around just for this simple task,
this tool is for you. Heck, even CMake needs a Linux environment for such a simple format.<br>

## Permissions & Ownership
Obviously debian packages depend on Unix concepts. For example do the files in the archive have Unix permission and ownership attributes, but we are creating the package outselves and can set whatever we want.

For now, the tool hardcodes 755 for folders and 644 for files. The library lets you choose. UID/GID are configureable in both, but only globally (for all files in the package).

You can always fix them in the postinst script, as some package managers don't honour the permissions anyways (there are good reasons).

## Tool usage
```
debbuild Utility Version 1.0.0.0 (C) Jakob K.

Synopsis: debbuild <folder> <outfile> <uid> <uname> [gid] [gname]

Options:
    folder    The folder containing the control and data folders.
    outfile   The name and path for the resulting package.
    uid       The owning user id for the contained files.
              If not otherwise specified, also the group id.
    uname     The owning users name.
              If not otherwise specified, also the group name.
    gid       Specifies the group id.
    gname     Specifies the group name.

```

## Expected input
The tool and libary require the package folder containing a `control` and `data` folder. They do not care about control, postinst, etc. files (yet?). Their creation is up to the user, the tool is just a packer.

+ PackageFolder\
  + control\
  + data\
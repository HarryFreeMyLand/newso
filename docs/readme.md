# What is FreeSO?
FreeSO is an implementation of both the client and server aspects of the Sim Online games.
NewSO is a fork of this project, with different goals from FreeSO, specifically:

* Documentation
* Improvements

Note: You will still need a copy of the original game to be able to use FreeSO/NewSO

# File Hierarchy
[Most of the sourcecode is located in /src](https://github.com/antonwilc0x/newso/tree/master/Src)
The project is composed of C# code, much of which is based on older code dating back to the Sim's Restoration Project, based on a really old .NET Framework. Over time it's being upgraded to modern standards.
As a result, things are inconsistent because many parts have been hacked together, though we are trying to unify the coding styles.
> An example of this is the src directory, you can already see two distinct coding styles in that some directories are named FSO and others tso, which is a throwback to the days where the code was based off a previous project called Project Dollhouse
The current biggest challenge is to find out exactly how the undocumented code interacts as part of the whole.

## EOD
Currently the best documented part is how to add new/custom UI to objects, based on a plugin style architecture. [For more information, consult FreeSO's website](http://freeso.org/custom-eod-guide/)


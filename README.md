# Heibroch.HotSwap
A solution to assembly hotswapping!
Threw this together in order to resolve a informal challenge :)

What it does it that you can have 2 instances of the same assembly (and its dependencies) in two subfolders of a specified root-directory.
You then replace files in the non-active instance. HotSwappedObject will detect this change and dynamically switch to the newly modifed instance.

Note that you can throw as many calls to your object as you want. The hotswap object will automatically halt the thread the operations are being called on
and then switch the instance and spin up a new object before continuing. 

Gudmundur, let me do a demonstration next time - it's actually pretty cool.

==Author==
[mailto:schoen@defectivestudios.com Matt Schoen] of [http://www.defectivestudios.com Defective Studios]

= Intro =
''At long last... a Unity GameObject merge/diff tool!''

Every team working with Unity knows the pain: you and another teammate both make major changes to a scene, or to the same prefab, and when it comes time to merge your changes into the game, it's "mine" or "theirs".  You can save your changes out as prefabs and bring them in manually, or or keep one scene and have to duplicate the other scene's work, or simply fight to the death to decide whose work stays. Bah!

But no longer!  We're pleased to introduce to you: UniMerge, a handy tool for merging objects, prefabs, and whole scenes, with color-coded diffs directly in the Unity editor.  This way you can manipulate your objects with the interface you're already comfortable with, and you can see the results in the scene instantly!  The tool comes with some scripts and instructions to integrate with TortoiseGit and Git on Windows (more VCS and OS support soon!).  The workflow is also compatible with Unity's Asset Server.

''But why should this amazing tool exist?''

Merging Unity assets used to be impossible when scenes were binary-only. So, we dreamt of a tool to allow a user to compare two GameObjects and everything about them, in the same interface you use to edit the original objects. Now it exists, hooray!  

But, why exactly can't we merge via text?  There's a lot of extraneous information which is useful to Unity but really doesn't matter to us, and it seems that Unity does not maintain consistent ordering when saving scenes and prefabs (the real kicker).  When you go back to the merged scene, you can end up with duplicated and/or mangled objects in the scene.  

Now, we try to keep Git out of the equation and merge everything within Unity.  As long as you have a copy of each of the the scenes or prefabs in question, you can get your changes back.  As long as you can see the object in Unity, you can actually make use of those changes and merge 'em in!

Anyway enough about the tool, here's how to use it!  Let's start with a very basic case.  We have two versions of some object and want to see if/how they are different.  Our job is to make them the saaaaame.

= How To Merge Objects =
[[Image:Breakdown.png|thumb|upright=2.8|alt=The Object Merge Window|The Object Merge Window]]
# Open the ObjectMerge Window (Window -> Object Merge -> Object Merge)
# Drag your objects from the scene to "mine" and "theirs" at the top of the window

That’s it!  Now you’ll see the interface in all its glory. If your objects are simple, there’s not much to inspect. If you have a more complicated object, you might get something like this:

So what’s going on here?? I’ll break it down:
# '''Options''': ''Deep Copy'' will set references to objects you just copied in the object it was copied to.  Disable this if you don't want this behavior or if copying a large, complex object crashes Unity (sorry about that).  ''Log'' will enable logging on certain operations.  ''Compare Attributes'' will enable the inclusion of GameObject attributes (name, layer, tag, etc.) in the comparison algorithm.  ''Expand Differences'' will open up any objects (and their parents) that have differences.
# '''Filters''': The drop-downs on the right will list every component type available in your project.  Use them like you use the layer mask GUI on lights and cameras to include/exclude component types from the comparison.  Those components will still show up red if there are differences, but they won't be checked in their parent object.  All three lists are checked simultaneously, and have to be broken up because Unity's mask GUI can only handle 31 items at a time.
# '''Root object slots''':  Drop your root objects here.  Once you fill both, the merge interface will magically appear!  Use the clear button if you want to get rid of the interface (for some reason).  The object picker breaks because we’re using a GUISkin for the window.  The ''PrefabInstance'' you see below the object field tells you the prefab state of the object (this one happens to have no prefab).
# '''Foldouts''': GameObjects, Components, and some properties will be listed with a little arrow next to them.  As with everywhere else in the Unity interface, this arrow expands the contents below.  Note that all foldouts here are tied to both sides at once.  This is intentional in order to keep the layout sane and make it so that blank space means the object is actually missing.  Holding alt while clicking on a GameObject foldout will expand/collapse all of its children with it.
# '''Ping Button''': The Ping button will ping the object in the hierarchy, in case you need to do something with it in the scene.
# '''Mid buttons''': These have a lot of different states.  Generally speaking, the one on the left pertains to an object on the left, and the right to the right.  There’s one situation where a button exists but doesn't work - this is temporary.  If you try to copy an object to an empty space which is also empty in the parent, you’ll get a warning telling you to copy the parent over.  Moving on...
#* This set of buttons applies to a GameObject.  Soon I’ll be differentiating the rows visually, but either way, it is important to understand that these arrows do specific things. When copying between two existing GameObjects, this copies all components and properties in the direction of the arrow.  If you click the right button, the left side will override the right, and vice versa.
#* These arrows copy data between components.  All properties are copied.
#* These arrows copy data between properties.  Only the value at this row is copied.
#* Sometimes, when one side is missing, you’ll get an X on the side that has the object.  This will delete that object (or component), thus making that row the same by eliminating it.
#* Sometimes you won’t see buttons. This is because the opposite object is missing entirely (not just missing that component).  You will also get this for any properties of a missing component. You need to copy the component over to get those properties--you can’t copy them into nothing.
# '''Show/Hide and Reset''': The column on the right has two buttons that affect the entire row (not just one object). To differentiate between children and components, components are shown/hidden by a button instead of the foldout. There is a gap after the components to show you where they end and the children begin.  The ''R'' Button will reset the row, which will re-do the comparison and update for any changes made outside of the window.  Note that this will clear the foldout state for all children.  Eventually, the window will always refresh itself, but because of this child closing behaviors, some resets are avoided to keep the interface from disappearing when you make a change.
# '''Mismatched row''': If an object or component doesn't have a spouse (matching object on the other side), you will get an ''X'' button on the side where it exists.  This button will destroy the object or component, thus making that row "equal" by getting rid of it.  You will not get copy buttons when showing components of a mismatched object.  You must first copy the object over to copy the components.  Note that you'll see empty space on the side without the object.

= Pro Tips =
# You can pull up any scene or prefab from any state by using TortoiseGit’s Repo Browser.  Open the log, find the commit you want, right click it and choose Repo Browser.  Then find the file you want, and Save as... to the assets folder. Then merge! Good Luck!
# The conflict check (red/green background) compares all components, fields, and child objects, but the part of the code that checks if children are “the same” only compares names.  The difference here is that in cases where an object on the left is not matched on the right, only names are considered.  There is no “history” to tell us whether two copies of a object used to be the same thing, so this is the best we can come up with so far.  Bear in mind that re-naming sub-objects will cause them to be treated as “unmatched” in this way.
# Don’t forget to use the normal Unity editor!  You can take advantage of multi-edit and other editor scripts you’ve devised and the rest, once you’ve found the differences with this window.  Sometimes it’s much easier to do things conventionally :)
# Make use of the alt-click and Expand Differences functionality.  Don't sit there folding out each object one-by-one when you can expand/collapse all by holding alt, and selectively expand to differences with the Expand Differences button.
# Be careful using the tool on large, complex scenes (more than a few thousand objects).  The tool will process in the background, but can still take a long time (over a minute) to refresh the comparison state of each object.  Break your scene up into chunks.  If you do decide to take the plunge and work on the whole thing at once, the comparison will run faster if you collapse the GUI down to showing just the top row.

= Scene Merging =

Scene merging is pretty simple, too.  It also has a very similar workflow!  Simply do the following:

# Open the SceneMerge Window (Window -> Object Merge ->Scene Merge)
# Drag in your scenes
# Press the Merge button
# Do your merge in the ObjectMerge window
# Click ''Unpack Mine'' or ''Unpack Theirs'' depending on which side you want as the main scene.

You should be back where you started, with a merged scene :)

Note that you need to merge dependencies (a.k.a. prefabs) first.  If you try to merge a scene with prefabs containing version control mockup, Unity will spam the log with warnings, and the merge grinds to a halt.  In version 1.6, UniMerge will abort the merge and warn you about this problem.

= Config Window =
Generally speaking, you shouldn’t have to touch this window.  Everything here has to do with loading the custom skin for the colored backgrounds.  The text fields set name/path values that are used to load the skin and reference the custom styles that define the background colors.  There are 8 texture fields below these to override the default colors.

If you are red/green colorblind, just go ahead and drop some alternate colors into these textures.  You can either create a new set of 8 images, or edit the images themselves directly. If anyone comes up with some good alternate color schemes, I’d be happy to include them and a drop-down to switch between pre-made schemes.

= VCS/SCM Integration =
Note that certain example paths below need to be changed based on your folder structure.  Also, the Windows merge-unity.js script assumes Unity is installed on the C drive in the default location.  If you want it to point to a specific version of Unity, or if you have installed Unity somewhere other than the default, ''you will have to modify merge-unity.js''.

== Git for Windows ==
This is where it gets a bit complicated.  This was my first experience putting a custom merge driver into gitconfig, so if I’m doing it wrong, I won’t be surprised.  From what I can tell, there are two files you have to modify.  Add the following lines to the following files:

* <Git_Install_Path>/etc/gitconfig
<syntaxhighlight lang="csharp">
*.unity		merge=unity
*.prefab	merge=unity
</syntaxhighlight>
* <Local .git folder>/config
<syntaxhighlight lang="csharp">
[merge "unity"]
	name = Unity merge
	driver = wscript.exe "D:/Documents/GimbalCop/GimbalCop/merge-unity.vbs" %A %B
</syntaxhighlight>

== TortoiseGit ==
If you want to have this happen with TortoiseGit, go to TortoiseGit->Settings->Diff Viewer->Advanced... and add two rules:

<syntaxhighlight lang="csharp">
Extension or mime-type: .unity
External Program: wscript.exe "D:\Documents\CosmoKnots\CosmoKnots\merge-unity.vbs" %base %mine
</syntaxhighlight>
<syntaxhighlight lang="csharp">
Extension or mime-type: .prefab
External Program: wscript.exe "D:\Documents\CosmoKnots\CosmoKnots\merge-unity.vbs" %base %mine
</syntaxhighlight>

Next, go to TortoiseGit->Settings->Diff Viewer->Merge Tool->Advanced... and add two rules:

<syntaxhighlight lang="csharp">
Extension or mime-type: .unity
External Program: wscript.exe "D:\Documents\CosmoKnots\CosmoKnots\merge-unity.vbs" %mine %theirs
</syntaxhighlight>
<syntaxhighlight lang="csharp">
Extension or mime-type: .prefab
External Program: wscript.exe "D:\Documents\CosmoKnots\CosmoKnots\merge-unity.vbs" %mine %theirs
</syntaxhighlight>

Make sure you swap out D:\Documents\CosmoKnots\CosmoKnots\ with the path to your Unity project folder. Note that the script no longer uses relative paths, so you can put it wherever you want. You do, however, now have to go change the ProjectBase value to the path of your project folder.


I obviously have to figure out a better way to point to that script path.  Either way, that path will be the path to a VBS file that is run by the Windows Scripting Host (should be on all versions that support Unity).  This script will check if Unity is running and, depending on the state, do one of two things.  It will either (If Unity isn’t running) boot Unity and trigger the scene merge, or it will drop a file called merges.txt into the Assets folder.  If you have the SceneMerge window open, it will listen for that file drop and trigger a merge as soon as it updates.

This seems to work only on resolving conflicts.  I’m not exactly sure how to make it do this every time a scene or prefab merge happens.  The script is called but points at three files which don't exist.
Also, if you have unresolved conflicts in your ProjectSettings.asset file, Unity will assume a version mismatch and ask you to upgrade your project (don't do it!).  Not sure how to get around this.
Also, it only works on Windows.  It won’t be too big a deal to get it working on Mac/Linux...Oh right, no Unity Editor on Linux ;P

Right now, the script doesn't clean up after itself since I don’t want it to be destructive while I work on it.  So you’ll have to delete the .REMOTE, .BASE, etc. files generated by Git in this process.

Not sure if there’s an automated way of setting up this process.  In lieu of tortoisegit, setting up a mergetool has always been kind of the bane of my git existence.  It’s pretty obscure and nobody seems to want to help you do it =/

== OS X and Git ==
superprat has been kind enough to create a [https://github.com/superprat/unimergemacdriver github project] for an git integration on OS X.  His readme should explain how to set it up.
The merge-unity.sh script has been included in the latest version of the package.

== PlasticSCM on Windows ==
Just like with TortoiseGit, you want to have Plastic run the merge-unity.js script through wscript.exe.  Here is the command for the merge. You should set up two rules with this command, one for .unity, one for .prefab.
<syntaxhighlight lang="csharp">
wscript.exe "D:\Documents\CosmoKnots\CosmoKnots\merge-unity.vbs" @sourcefile @destinationfile
</syntaxhighlight>

Make sure you swap out D:\Documents\CosmoKnots\CosmoKnots\ with the path to your Unity project folder.  Note that the script no longer uses relative paths, so you can put it wherever you want.  You do, however, now have to go change the ProjectBase value to the path of your project folder.

== Other VCS Integration ==
Integrating this tool into other VCS solutions should be very straightforward.  I'm not sure exactly how other systems specify mergetool overrides, but regardless you should be able to point them at a path which will open the bridge script.  We could either create a specific bridge for each VCS, or merge them all into a single script which would auto-detect which software was calling it.  Contact me if you want help setting up your VCS with the ObjectMerger.  Or, if you’ve figured it out already, I'd greatly appreciate it if you shared your bridge script and some instructions so that other users of your VCS don’t have to re-invent the wheel.

Anyways, that should be all.  Happy Merging!

I can't include a zip file in the AssetStore package, so here's a download link: [[File:Git_scripts.zip]]

= Tests =
The following tests will indicate whether any code changes have created issues in the tool.  For all merge tests, I’m going to assume mine -> theirs for the sake of semantics.  They should obviously work the same way in both directions
*Merge root object
** Should destroy “theirs” object and duplicate “mine.” Check that there are no errors
* Merge sub-object with no spouse
** Should duplicate “mine” and make it a child of the object in “theirs” that corresponds to the first object’s parent.  If “theirs” has no such parent, a LogWarning will fire explaining why the merge can’t be done.  Additionally, if any GameObjects or Components in the first object’s tree are referenced by any object within “mine,” the corresponding references should be set in “theirs”.  If log is enabled it should print out all of the references that were set.
* Merge sub-object with spouse
** Should copy all components, properties, and children into the spouse.  If log is enabled, any references which are set will be logged.
* Remove sub-object with no spouse
** Should destroy the sub-object and set all references to it to null.  If logging is enabled, refs set to null are logged.
* Merge component with no spouse
** Should create a new component on the other object and copy all property values. Also sets any references to the new component. If log is enabled, all set references will be logged.
* Merge component with spouse
** Should copy all property values over.  No references are set, nothing is logged.
* Remove component with no spouse
** Should destroy the component and set any references to it to null.  If logging is enabled, refs set to null are logged
* Merge property
** Should set the value of the “theirs” property to that of “mine”.  If the property references a component or GameObject, it attempts to copy the referenced object to the corresponding parent on “theirs.” This will also find and set references like a regular copy.  If logging is enabled, ref settings and copy are logged.
* Refresh button
** Should refresh the object and all of its children.  This will collapse children, and should set background colors right.
* Merge Scenes
** You shouldn’t be able to click Merge or the Unpack buttons at first.  Once you drop in two scenes (you can actually use the same scene twice if you’re just testing), Merge should become enabled.  Once you hit merge, the ObjectMerge window will open, and you’ll be able to click the Unpack buttons.  As soon as you delete mine or theirs (or open another scene), the corresponding button should ghost out.  Clicking Unpack should unpack one or the other object  and delete both container objects.
* Git integration
** This is a little tough to test.  Git should try to open Unity if it isn’t running whenever a scene or prefab is merged, and/or when you try to resolve a conflicted merge.  In some cases, the git integration doesn’t work because it doesn’t create copies of the scene to be opened in Unity
If Unity is already open when the merge happens, and you have the SceneMerge window up, it will listen for a certain file that the bridge script will create and open the scenes or prefabs automatically.

= Known Issues =
* Actions will not update an object’s grandparents’ conflict state.  You will have to manually hit refresh.  This is not done automatically to avoid collapsing the children.
* Clicking a foldout label won’t toggle it.  This is Unity’s fault.
* The license is a little sketchy.  The project isn’t strict open-source but I want to allow people to modify the code and share their modifications. There must be a license for this out there...
* Sometimes the alternating-darkness for the background doesn’t quite alternate right.... not sure what’s up with that.
* You can't use this to recover missing references due to asset ID mismatches.  In other words, if you bring in the original scene and it has missing references (sometimes this even happens with references within the scene), the tool won't help you find them.  This is a broader issue with Unity's asset management which is hard to trace and ultimately probably impossible to solve without using Asset Server to centrally assign asset IDs.  Any advice on how to handle this better is welcome.
* As of 4.6, non-alphanumeric sorting of the heirarchy means that if you change order, the merger won't be able to match objects

= Roadmap =
What’s next?
* VCS integration
** A helpful user has contributed an OS X git bridge. Still waiting on demand for systems like Perforce or Mercurial.  Note: for systems like SVN that don't handle merging, there's nothing to be done.  The tool works as well as possible as-is!
* Better filtering
** The current UI is a little awkward, and you can only filter by component type.  I’d also like to filter out property names and/or object names
* nGUI support
** Currently, due to the nature of how nGUI generates meshes for its objects, nGUI components will always show up conflicted.  The current workaround is to use the component type filters to ignore the generated data, but a better workaround is possible.
* Merging lists of objects
** Currently, for the purposes of merging two parallel lists of children, only object names are compared.  For example, we have one object with Child 1, Child 2, and Child 3, and another with Child 2, Child 3, and Child 4.  As it stands, we’ll see a list with 4 items.  Since there is no child named Child 1 it will be alone on the left, and likewise Child 4 will be alone on the right.  It might be appropriate to merge the list differently, and we’d be able to make a better decision with information about the object’s history.  Anyway this is an area for improvement, but I have no idea what would be better.
* "Smarter" merging/merge options
** Currently the “merge” buttons in the middle will simply copy the object over wholesale.  As a first step in the direction of “smart” merging, the tool will try to maintain references to the object and its children and components, but there could be other merge strategies that could avoid overwriting other objects, or something like that.
* Skinning/GUI
** The GUI is rather simple as-is but is still pretty busy.  I’m constantly thinking about ways to improve it.
* Refresh behavior
** When changes are made, I avoid refreshing from the root for two reasons: firstly, for very complicated objects, refresh can take a few seconds.  Secondly, the current refresh function will re-create the holder objects that store whether a row is collapsed or open.  If I refreshed the whole tree on every action, you would have to keep re-opening the tree to where you were.  Likewise, whenever changes are made in the scene, you have to manually refresh the row in the merge window.  Eventually, I’d like to be able to do away with the refresh button altogether, and auto-refresh whenever is needed.
* Documentation
** I hope to add some more screencasts and better screenshots of tool in action.  The [https://www.youtube.com/watch?v=SWmca1Ozntw demo screencast] is a good start, though!
* Code cleanup
** The way that GameObject attributes are handled is stupid.  It should be less stupid
** Along with Refresh(), the various functions around Copy could be re-organized and made more clear.
** FindRefs might work better and faster if it just searches from the root instead of the current object
** Might have to turn FindAndSetRefs into a coroutine for really large objects
** Refresh and FindAndSetRefs are O(n^2).  I might be able to make this faster
** Much of the code is broken up into a pair of procedures: one for mine and one for theirs. These could easily be extracted and turned into a single function with two parameters.

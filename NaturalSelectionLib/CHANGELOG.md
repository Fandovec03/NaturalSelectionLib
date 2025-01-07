0.6.3 <br>
	- Fixed NRE from FilterEnemyList (Again)
0.6.2 <br>
	- Fixed critical issue where GetCompleteList would directly modify RoundManager enemy list instead of making a copy, causing unexpected behavior
	- Merged GetOutsideEnemyList and GetInsideEnemyList into single method
	- Fixed NRE from FilterEnemyList
	- Fixed GetEnemiesInLOS ordering enemies from farthest to nearest
	- Fixed order of conditions in FindClosestEnemy
	- Optimized methods and updated logs
0.6.1 <br>
	- Updated __DebugStringHead__ <br>
0.6.0 <br>
	- Added blacklist to filterEnemyList<br>
	- - Searches scan node and enemy type to find match and filter out<br>
0.5.1 <br>
	- Publicized globalEnemyList dictionary <br>
0.5.0 <br>
	- Added a dictionary for enemies of the same type to share the same enemy list <br>
	- FilterEnemyList when passed null for the targeted enemy types will not filter the list by enemy types <br>
0.4.0 <br>
	- Removed enemyList. <br>
	- Removed EnemyListUpdate function <br>
	- Reworked GetCompleteList. The function now removes enemies not meeting conditions from a copy of ingame enemy list instead of adding them to a new list when meeting the conditions <br>
0.3.0 <br>
	- Fixed FindClosestEnemy not taking into account if imported closestEnemy is dead or not resulting in the enemies always targeting dead previously targeted enemy<br>
	- Added argument controlling if FindClosestEnemy should include dead enemies.<br>
0.2.4 <br>
	- Added a bool to GetCompleteList for excluding, including or returning dead enemies in the output.<br>
0.2.3 <br>
	- Wrong DLL<br>
0.2.2 <br>
	- Fixed CHANGELOG formating<br>
	- Fixed BepInEx dependency<br>
0.2.1<br>
	- Wrong Dll<br>
0.2.0<br>
	- Library proven to work. Releasing public build<br>
0.1.3<br>
	- Publicized all Methods and Fields<br>
0.1.2<br>
	- Removed all Harmony references<br>
	- Publicized all methods<br>
0.1.1<br>
	- Fixed the main class being internal<br>
0.1.0<br>
	- Initial test release.<br>
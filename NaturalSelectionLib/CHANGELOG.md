0.4.0 <br>
	- Removed enemyList.
	- Removed EnemyListUpdate function.
	- Reworked GetCompleteList. The function now removes enemies not meeting conditions from a copy of ingame enemy list instead of adding them to a new list when meeting the conditions.
0.3.0 <br>
	- Fixed FindClosestEnemy not taking into account if imported closestEnemy is dead or not resulting in the enemies always targeting dead previously targeted enemy.<br>
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
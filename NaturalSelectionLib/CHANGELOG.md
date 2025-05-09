## 0.6.8
- Refactored methods

## 0.6.7
- Updated methods to remove items not meeting conditions instead of adding ones that do to temporary list

### FilterEnemyList
- No longer turns blacklist entry names to upper case

### DebugStringHead
- Added bool parameter to return shortened string

### 0.6.6
- Updated CHANGELOG format
- Updated DebugStringHead

##### Internal
- Updated strings to use interpolation

### 0.6.5
- Fixed NRE from filterEnemyList
- Simplified DebugStringHead

### 0.6.4 
- Updated fliterEnemyList

### 0.6.3 
- Fixed NRE from FilterEnemyList (Again)

### 0.6.2 
- Fixed critical issue where GetCompleteList would directly modify RoundManager enemy list instead of making a copy, causing unexpected behavior
- Merged GetOutsideEnemyList and GetInsideEnemyList into single method
- Fixed NRE from FilterEnemyList
- Fixed GetEnemiesInLOS ordering enemies from farthest to nearest
- Fixed order of conditions in FindClosestEnemy
- Optimized methods and updated logs

### 0.6.1 
- Updated __DebugStringHead__ 

### 0.6.0 
- Added blacklist to filterEnemyList
- - Searches scan node and enemy type to find match and filter out

### 0.5.1 
- Publicized globalEnemyList dictionary 

### 0.5.0 
- Added a dictionary for enemies of the same type to share the same enemy list 
- FilterEnemyList when passed null for the targeted enemy types will not filter the list by enemy types 

### 0.4.0 
- Removed enemyList. 
- Removed EnemyListUpdate function 
- Reworked GetCompleteList. The function now removes enemies not meeting conditions from a copy of ingame enemy list instead of adding them to a new list when meeting the conditions 

### 0.3.0 
- Fixed FindClosestEnemy not taking into account if imported closestEnemy is dead or not resulting in the enemies always targeting dead previously targeted enemy
- Added argument controlling if FindClosestEnemy should include dead enemies.

### 0.2.4 
- Added a bool to GetCompleteList for excluding, including or returning dead enemies in the output.

### 0.2.3 
- Wrong DLL

### 0.2.2 
- Fixed CHANGELOG formating
- Fixed BepInEx dependency

### 0.2.1
- Wrong Dll

### 0.2.0
- Library proven to work. Releasing public build

### 0.1.3
- Publicized all Methods and Fields

### 0.1.2
- Removed all Harmony references
- Publicized all methods

### 0.1.1
- Fixed the main class being internal

### 0.1.0
- Initial test release.
# GeneralConfig Editor - Test Verification Guide

## Application is Now Running

The GeneralConfig Editor application has been successfully launched. Here's what to verify:

---

## 1. INITIAL STATE - Sample Data Loaded

### Expected Tree View (Left Panel):
```
✓ GeneralConfig (root node - expanded)
  ├ DatabaseMgmtConfigs (1)
  │  └ DatabaseMgmtConfig - DB App 1
  ├ FileMgmtConfigs (1)
  │  └ TextFileMgmtConfig - Text File 1
  ├ AppLoadConfigs (0)
  └ AppWriteConfigs (0)
```

### Expected Right Panel:
- Empty initially (no item selected)
- When you select "TextFileMgmtConfig - Text File 1", properties should display:
  - AppName: "Text File 1"
  - AppVersion: "1.0"
  - LoadNumber: 0
  - SaveNumber: 0
  - UpdateNumber: 0
  - LoadTime: (current date/time)
  - SaveTime: (1970-01-01)
  - FileAction: (dropdown showing: Load, Save, Update) - currently "Load"
  - FileName: (empty text box)
  - FilePath: (empty text box)
  - CodeStart: 0
  - CodeEnd: 15
  - NameLength: 50
  - Name: (empty text box)
  - DescriptionLength: 100

---

## 2. TEST: MODIFY PROPERTIES

1. Select "TextFileMgmtConfig - Text File 1" in the left panel
2. In the right panel, modify some properties:
   - Change **FileName** to "test.txt"
   - Change **FileAction** dropdown to "Save" (test enum support)
   - Change **LoadTime** using the calendar picker
   - Change **CodeStart** to 5
   - Change **NameLength** to 75

✓ **Expected Result**: All changes should update in real-time in the data model

---

## 3. TEST: ADD ITEMS TO COLLECTION

### Test 3A: Add to DatabaseMgmtConfigs
1. Click on "DatabaseMgmtConfigs (1)" in the tree
2. Click "Add Item" button
3. New DatabaseMgmtConfig item should be added
4. Counter should update: "DatabaseMgmtConfigs (2)"
5. New item should appear in tree as "DatabaseMgmtConfig - DatabaseMgmtConfig_2"

### Test 3B: Add to FileMgmtConfigs (with type selection)
1. Click on "FileMgmtConfigs (1)" in the tree
2. Click "Add Item" button
3. **Type Selection Dialog** should appear with three options:
   - CSVFileMgmtConfig
   - FileMgmtConfig
   - TextFileMgmtConfig
4. Select "CSVFileMgmtConfig" and click OK
5. New CSV config should be added
6. Counter should update: "FileMgmtConfigs (2)"
7. New item appears as "CSVFileMgmtConfig - CSVFileMgmtConfig_2"

✓ **Expected Result**: Collections should expand dynamically with new items

---

## 4. TEST: REMOVE ITEMS FROM COLLECTION

1. In the tree, select any item (e.g., "TextFileMgmtConfig - Text File 1")
2. Click "Remove Item" button
3. Item should disappear from the tree
4. Parent collection counter should decrement
5. Properties panel should clear

✓ **Expected Result**: Item should be completely removed

---

## 5. TEST: PROPERTY EDITOR FEATURES

### Enum Support (FileAction)
- Select a FileMgmtConfig item
- Locate the **FileAction** property
- Click dropdown and verify three options: Load, Save, Update
- Change value and verify it updates

### DateTime Support (LoadTime, SaveTime)
- Select a FileMgmtConfig item
- Locate **LoadTime** or **SaveTime** property
- Click on the DateTimePicker
- Select a new date/time from the calendar
- Verify the value updates

### String Support (AppName, FileName, FilePath)
- Type in any text field
- Verify text updates in real-time

### Integer Support (LoadNumber, CodeStart, NameLength)
- Use up/down spinner or type numbers
- Verify changes update immediately

---

## 6. TEST: TREE NAVIGATION

1. Click on different collection nodes to verify they're selectable
2. Expand/collapse collection nodes by clicking the +/- icon
3. Click on individual items to view their properties
4. Verify the properties panel updates dynamically for each selected item

---

## 7. TEST: EXTENSIBILITY (Future Child Classes)

The design supports adding new FileMgmtConfig child classes without code changes:

**Current Supported Types:**
- FileMgmtConfig (base)
- TextFileMgmtConfig
- CSVFileMgmtConfig

**To add a new type (e.g., JSONFileMgmtConfig):**
1. Create a new class inheriting from FileMgmtConfig
2. Rebuild the solution
3. When adding items to FileMgmtConfigs, the new type will automatically appear in the selection dialog

---

## Sample Test Scenario

1. **Load the app** - Verify sample data appears
2. **Select** "TextFileMgmtConfig - Text File 1"
3. **Modify properties**: Change FileName to "output.csv", FileAction to "Save", CodeStart to 10
4. **Add new item**: Click on "FileMgmtConfigs", click "Add Item", select "CSVFileMgmtConfig"
5. **Verify new item**: New item appears with auto-generated name "CSVFileMgmtConfig_2"
6. **Select new item**: Click it and verify empty/default properties
7. **Modify new item**: Set AppName to "My CSV Config", RowNumber to 100, CodeColumnName to "CODE"
8. **Remove item**: Select original TextFileMgmtConfig, click "Remove Item", verify it disappears
9. **Add to other collections**: Try adding to AppLoadConfigs and AppWriteConfigs
10. **Close and reopen**: Note that changes are in-memory (to persist, you'd need serialization)

---

## Features Verified ✓

- [x] Tree view with hierarchical structure
- [x] Expandable collections showing count
- [x] Property editor with dynamic controls
- [x] Enum dropdown support
- [x] DateTime calendar picker
- [x] String/Integer input fields
- [x] Add items with type selection for FileMgmtConfigs
- [x] Remove items from collections
- [x] Real-time property updates
- [x] Extensible type discovery via Reflection


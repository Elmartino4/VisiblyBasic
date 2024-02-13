'-- type: sorting
'-- input: Input
'-- main: SimpleBubbleSort

:__ANCHOR_0_SimpleBubbleSort__                         ' BEGIN SimpleBubbleSort
SET __TEMP_003__ 1                                     ' SET Swapped = True
EQUATE __TEMP_002__ __TEMP_003__                       ' ^
EQUATE __VAR_Swapped__ __TEMP_002__                    ' ^
EQUATE __VAR_Input__ __PARAM_0_SimpleBubbleSort__      ' GET Input()
:__ANCHOR_0_000__                                      ' WHILE Swapped == True
EQUATE __TEMP_004__ __VAR_Swapped__                    ' ^
SET __TEMP_005__ 1                                     ' ^
XOR __TEMP_004__ __TEMP_005__                          ' ^
ATZERO __TEMP_004__                                    ' ^
EQUATE __TEMP_000__ __TEMP_004__                       ' ^
ATZERO __TEMP_000__                                    ' ^
JUMP __ANCHOR_1_000__ __TEMP_000__                     ' ^
SET __TEMP_007__ 0                                     ' SET Swapped = False
EQUATE __TEMP_006__ __TEMP_007__                       ' ^
EQUATE __VAR_Swapped__ __TEMP_006__                    ' ^
SET __TEMP_009__ 0                                     ' SET Comparison = 0
EQUATE __TEMP_008__ __TEMP_009__                       ' ^
EQUATE __VAR_Comparison__ __TEMP_008__                 ' ^
:__ANCHOR_0_001__                                      ' WHILE Comparison < Input . length
EQUATE __TEMP_00A__ __VAR_Comparison__                 ' ^
EQUATE __TEMP_00B__ __VAR_Input__                      ' ^
CSUB __TEMP_00B__ __CONSTANT_1__                       ' ^
PNTSET __TEMP_00C__ __TEMP_00B__                       ' ^
CSUB __TEMP_00C__ __TEMP_00A__                         ' ^
EQUATE __TEMP_00A__ __TEMP_00C__                       ' ^
EQUATE __TEMP_001__ __TEMP_00A__                       ' ^
ATZERO __TEMP_001__                                    ' ^
JUMP __ANCHOR_1_001__ __TEMP_001__                     ' ^
EQUATE __TEMP_00E__ __VAR_Comparison__                 ' IF Input ( Comparison ) > Input ( Comparison + 1 )
EQUATE __TEMP_00F__ __VAR_Input__                      ' ^
ADD __TEMP_00E__ __TEMP_00F__                          ' ^
PNTSET __TEMP_00G__ __TEMP_00E__                       ' ^
EQUATE __TEMP_00H__ __VAR_Comparison__                 ' ^
SET __TEMP_00I__ 1                                     ' ^
ADD __TEMP_00H__ __TEMP_00I__                          ' ^
EQUATE __TEMP_00J__ __VAR_Input__                      ' ^
ADD __TEMP_00H__ __TEMP_00J__                          ' ^
PNTSET __TEMP_00K__ __TEMP_00H__                       ' ^
CSUB __TEMP_00G__ __TEMP_00K__                         ' ^
EQUATE __CONDITION_0_00D__ __TEMP_00G__                ' ^
ATZERO __CONDITION_0_00D__                             ' ^
JUMP __ANCHOR_0_00D__ __CONDITION_0_00D__              ' ^
EQUATE __TEMP_00M__ __VAR_Comparison__                 ' SET Temporary = Input ( Comparison )
EQUATE __TEMP_00N__ __VAR_Input__                      ' ^
ADD __TEMP_00M__ __TEMP_00N__                          ' ^
PNTSET __TEMP_00O__ __TEMP_00M__                       ' ^
EQUATE __TEMP_00L__ __TEMP_00O__                       ' ^
EQUATE __VAR_Temporary__ __TEMP_00L__                  ' ^
EQUATE __TEMP_00Q__ __VAR_Comparison__                 ' SET Input ( Comparison ) = Input ( Comparison + 1 )
SET __TEMP_00R__ 1                                     ' ^
ADD __TEMP_00Q__ __TEMP_00R__                          ' ^
EQUATE __TEMP_00S__ __VAR_Input__                      ' ^
ADD __TEMP_00Q__ __TEMP_00S__                          ' ^
PNTSET __TEMP_00T__ __TEMP_00Q__                       ' ^
EQUATE __TEMP_00P__ __TEMP_00T__                       ' ^
EQUATE __TEMP_00U__ __VAR_Comparison__                 ' ^
EQUATE __TEMP_00V__ __VAR_Input__                      ' ^
ADD __TEMP_00U__ __TEMP_00V__                          ' ^
LOCSET __TEMP_00U__ __TEMP_00P__                       ' ^
EQUATE __TEMP_00X__ __VAR_Temporary__                  ' SET Input ( Comparison + 1 ) = Temporary
EQUATE __TEMP_00W__ __TEMP_00X__                       ' ^
EQUATE __TEMP_00Y__ __VAR_Comparison__                 ' ^
SET __TEMP_00Z__ 1                                     ' ^
ADD __TEMP_00Y__ __TEMP_00Z__                          ' ^
EQUATE __TEMP_00a__ __VAR_Input__                      ' ^
ADD __TEMP_00Y__ __TEMP_00a__                          ' ^
LOCSET __TEMP_00Y__ __TEMP_00W__                       ' ^
SET __TEMP_00c__ 1                                     ' SET Swapped = True
EQUATE __TEMP_00b__ __TEMP_00c__                       ' ^
EQUATE __VAR_Swapped__ __TEMP_00b__                    ' ^
JUMP __ANCHOR_0_00D__ __CONSTANT_1__                   ' ENDIF
:__ANCHOR_0_00D__                                      ' ^
EQUATE __TEMP_00e__ __VAR_Comparison__                 ' SET Comparison = Comparison + 1
SET __TEMP_00f__ 1                                     ' ^
ADD __TEMP_00e__ __TEMP_00f__                          ' ^
EQUATE __TEMP_00d__ __TEMP_00e__                       ' ^
EQUATE __VAR_Comparison__ __TEMP_00d__                 ' ^
JUMP __ANCHOR_0_001__ __CONSTANT_1__                   ' ENDWHILE
:__ANCHOR_1_001__                                      ' ^
JUMP __ANCHOR_0_000__ __CONSTANT_1__                   ' ENDWHILE
:__ANCHOR_1_000__                                      ' ^
EQUATE __TEMP_00h__ __VAR_Input__                      ' RETURN Input
EQUATE __TEMP_00g__ __TEMP_00h__                       ' ^
EQUATE __RETURN_SimpleBubbleSort__ __TEMP_00g__        ' ^
JUMP __ANCHOR_1_SimpleBubbleSort__ __CONSTANT_1__      ' ^
:__ANCHOR_1_SimpleBubbleSort__                         ' END SimpleBubbleSort


'-- type: sorted-search
'-- input: Array, SearchValue
'-- main: BinarySearch

:__ANCHOR_0_BinarySearch__                             ' BEGIN BinarySearch
EQUATE __VAR_Array__ __PARAM_0_BinarySearch__          ' GET Array()
SET __TEMP_002__ 0                                     ' SET Low = 0
EQUATE __TEMP_001__ __TEMP_002__                       ' ^
EQUATE __VAR_Low__ __TEMP_001__                        ' ^
EQUATE __TEMP_004__ __VAR_Array__                      ' SET High = Array . length - 1
CSUB __TEMP_004__ __CONSTANT_1__                       ' ^
PNTSET __TEMP_005__ __TEMP_004__                       ' ^
SET __TEMP_006__ 1                                     ' ^
CSUB __TEMP_005__ __TEMP_006__                         ' ^
EQUATE __TEMP_003__ __TEMP_005__                       ' ^
EQUATE __VAR_High__ __TEMP_003__                       ' ^
SET __TEMP_008__ 0                                     ' SET Found = False
EQUATE __TEMP_007__ __TEMP_008__                       ' ^
EQUATE __VAR_Found__ __TEMP_007__                      ' ^
EQUATE __VAR_SearchValue__ __PARAM_1_BinarySearch__    ' GET SearchValue
:__ANCHOR_0_000__                                      ' WHILE High >= Low
EQUATE __TEMP_009__ __VAR_High__                       ' ^
EQUATE __TEMP_00A__ __VAR_Low__                        ' ^
CSUB __TEMP_00A__ __TEMP_009__                         ' ^
ATZERO __TEMP_00A__                                    ' ^
EQUATE __TEMP_009__ __TEMP_00A__                       ' ^
EQUATE __TEMP_000__ __TEMP_009__                       ' ^
ATZERO __TEMP_000__                                    ' ^
JUMP __ANCHOR_1_000__ __TEMP_000__                     ' ^
EQUATE __TEMP_00C__ __VAR_Low__                        ' SET Middle = ( Low + High ) / 2
EQUATE __TEMP_00D__ __VAR_High__                       ' ^
ADD __TEMP_00C__ __TEMP_00D__                          ' ^
SET __TEMP_00E__ 2                                     ' ^
DIV __TEMP_00C__ __TEMP_00E__                          ' ^
EQUATE __TEMP_00B__ __TEMP_00C__                       ' ^
EQUATE __VAR_Middle__ __TEMP_00B__                     ' ^
EQUATE __TEMP_00G__ __VAR_Middle__                     ' SET current = Array ( Middle )
EQUATE __TEMP_00H__ __VAR_Array__                      ' ^
ADD __TEMP_00G__ __TEMP_00H__                          ' ^
PNTSET __TEMP_00I__ __TEMP_00G__                       ' ^
EQUATE __TEMP_00F__ __TEMP_00I__                       ' ^
EQUATE __VAR_current__ __TEMP_00F__                    ' ^
EQUATE __TEMP_00K__ __VAR_SearchValue__                ' IF SearchValue < current
EQUATE __TEMP_00L__ __VAR_current__                    ' ^
CSUB __TEMP_00L__ __TEMP_00K__                         ' ^
EQUATE __TEMP_00K__ __TEMP_00L__                       ' ^
EQUATE __CONDITION_0_00J__ __TEMP_00K__                ' ^
ATZERO __CONDITION_0_00J__                             ' ^
JUMP __ANCHOR_0_00J__ __CONDITION_0_00J__              ' ^
EQUATE __TEMP_00N__ __VAR_Middle__                     ' SET High = Middle - 1
SET __TEMP_00O__ 1                                     ' ^
CSUB __TEMP_00N__ __TEMP_00O__                         ' ^
EQUATE __TEMP_00M__ __TEMP_00N__                       ' ^
EQUATE __VAR_High__ __TEMP_00M__                       ' ^
JUMP __ANCHOR_2_00J__ __CONSTANT_1__                   ' ELSEIF SearchValue == current
:__ANCHOR_0_00J__                                      ' ^
EQUATE __TEMP_00P__ __VAR_SearchValue__                ' ^
EQUATE __TEMP_00Q__ __VAR_current__                    ' ^
XOR __TEMP_00P__ __TEMP_00Q__                          ' ^
ATZERO __TEMP_00P__                                    ' ^
EQUATE __CONDITION_1_00J__ __TEMP_00P__                ' ^
ATZERO __CONDITION_1_00J__                             ' ^
JUMP __ANCHOR_1_00J__ __CONDITION_1_00J__              ' ^
SET __TEMP_00S__ 1                                     ' RETURN True
EQUATE __TEMP_00R__ __TEMP_00S__                       ' ^
EQUATE __RETURN_BinarySearch__ __TEMP_00R__            ' ^
JUMP __ANCHOR_1_BinarySearch__ __CONSTANT_1__          ' ^
JUMP __ANCHOR_2_00J__ __CONSTANT_1__                   ' ELSE
:__ANCHOR_1_00J__                                      ' ^
EQUATE __TEMP_00U__ __VAR_Middle__                     ' SET Low = Middle + 1
SET __TEMP_00V__ 1                                     ' ^
ADD __TEMP_00U__ __TEMP_00V__                          ' ^
EQUATE __TEMP_00T__ __TEMP_00U__                       ' ^
EQUATE __VAR_Low__ __TEMP_00T__                        ' ^
:__ANCHOR_2_00J__                                      ' ENDIF
JUMP __ANCHOR_0_000__ __CONSTANT_1__                   ' ENDWHILE
:__ANCHOR_1_000__                                      ' ^
SET __TEMP_00X__ 0                                     ' RETURN False
EQUATE __TEMP_00W__ __TEMP_00X__                       ' ^
EQUATE __RETURN_BinarySearch__ __TEMP_00W__            ' ^
JUMP __ANCHOR_1_BinarySearch__ __CONSTANT_1__          ' ^
:__ANCHOR_1_BinarySearch__                             ' END BinarySearch


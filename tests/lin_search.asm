'-- type: unsorted-search
'-- main: LinearSearch

:__ANCHOR_0_LinearSearch__                             ' BEGIN LinearSearch
EQUATE __VAR_Array__ __PARAM_0_LinearSearch__          ' GET Array()
EQUATE __VAR_Value__ __PARAM_1_LinearSearch__          ' GET Value
SET __TEMP_002__ 0                                     ' SET Found = False
EQUATE __TEMP_001__ __TEMP_002__                       ' ^
EQUATE __VAR_Found__ __TEMP_001__                      ' ^
SET __TEMP_003__ 0                                     ' FOR i = 0 TO Array . length - 1
EQUATE __VAR_i__ __TEMP_003__                          ' ^
:__ANCHOR_0_000__                                      ' ^
EQUATE __TEMP_004__ __VAR_Array__                      ' ^
CSUB __TEMP_004__ __CONSTANT_1__                       ' ^
PNTSET __TEMP_005__ __TEMP_004__                       ' ^
SET __TEMP_006__ 1                                     ' ^
CSUB __TEMP_005__ __TEMP_006__                         ' ^
EQUATE __TEMP_0_000__ __TEMP_005__                     ' ^
CSUB __TEMP_0_000__ __VAR_i__                          ' ^
ATZERO __TEMP_0_000__                                  ' ^
JUMP __ANCHOR_1_000__ __TEMP_0_000__                   ' ^
EQUATE __TEMP_008__ __VAR_i__                          ' IF Array ( i ) == Value
EQUATE __TEMP_009__ __VAR_Array__                      ' ^
ADD __TEMP_008__ __TEMP_009__                          ' ^
PNTSET __TEMP_00A__ __TEMP_008__                       ' ^
EQUATE __TEMP_00B__ __VAR_Value__                      ' ^
XOR __TEMP_00A__ __TEMP_00B__                          ' ^
ATZERO __TEMP_00A__                                    ' ^
EQUATE __CONDITION_0_007__ __TEMP_00A__                ' ^
ATZERO __CONDITION_0_007__                             ' ^
JUMP __ANCHOR_0_007__ __CONDITION_0_007__              ' ^
SET __TEMP_00D__ 1                                     ' SET Found = True
EQUATE __TEMP_00C__ __TEMP_00D__                       ' ^
EQUATE __VAR_Found__ __TEMP_00C__                      ' ^
JUMP __ANCHOR_0_007__ __CONSTANT_1__                   ' ENDIF
:__ANCHOR_0_007__                                      ' ^
ADD __VAR_i__ __CONSTANT_1__                           ' NEXT i
JUMP __ANCHOR_0_000__ __CONSTANT_1__                   ' ^
:__ANCHOR_1_000__                                      ' ^
EQUATE __TEMP_00F__ __VAR_Found__                      ' RETURN Found
EQUATE __TEMP_00E__ __TEMP_00F__                       ' ^
EQUATE __RETURN_LinearSearch__ __TEMP_00E__            ' ^
JUMP __ANCHOR_1_LinearSearch__ __CONSTANT_1__          ' ^
:__ANCHOR_1_LinearSearch__                             ' END LinearSearch


   M S B A M L   `   `   `  ����A  =bimkit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nulli9clr-namespace:bimkit.sheet_tools.titleBlockEditor_feature+bimkit.sheet_tools.titleBlockEditor_feature  5   	   G    Abimkit.sheet_tools.titleBlockEditor_feature.SheetPropertiesWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Klocal9clr-namespace:bimkit.sheet_tools.titleBlockEditor_feature     5   	     � Title$  Sheet��5   	    � WindowStyle$
 None=�6    � AllowsTransparency . $	��450��6D    � SizeToContent$ Height=�5	   	   $	��100��6    .��5
      �� 
���   �$��1q�6!   ._�5   
   � $��	0,0,-1,-1q�6   .+�5      �� -   5        $��40��5      $	��Top=�6!   ��5      �� 5      .��5      �  � Color$ #3264c8��6'    � Offset$ 0Q�67   � 5      $ #090b3c��6'   $ 1Q�67   5      ._�5      � .+�5      �� $��BIMKit��6    
�������$��20(�6A   $��Center=�6O   $��10,0,0,0q�6j   �� 5      -   5        $��X��5      $��20��6)   $��20��64   $��Right=�6@   $��Center=�6\   $��0,0,10,0q�6w   
������ 
�������
������ 6�   5      5      �� 5      $��
26,10,26,0q�6   .+�5      �� $/��)Edit Common Properties of Selected Sheets��6   $
��Bold"�6M   $��	0,50,0,10q�6_   &   System.Windows.Controls.DataGrid  5      -   5        $��PropertiesDataGrid��5         AutoGenerateColumns .     CanUserAddRows .  $��0,0,0,10q�6h   $
��Auto��6z      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn	   Header$	 Property��6-   
   
IsReadOnly
 .    Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6j   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6?   �� *Name+6j     5      $	 Value��6-   $ * 6c    6<   �� *Value+ �� Mode$ TwoWay=�6c   5!      �� 5"      $
��Left=�6   $��	0,0,0,-30q�67   $��Bottom=�6J   �� 5#      -   5        $��https://www.bimcap.com/?�5#       .�6r   .�5$      �� $�Copyright © 2024 BIMCAP��6   5%      5&      �� 5'      -   5        $��Save Changes��5'      $��Right=�6J   $��
0,10,90,10q�6f   $��80��6z   
��� � �6�   �� 5(      -   5        $��Discard��5(      $��Right=�6B   $��
0,-30,0,10q�6^   $��80��6r   
��� ���6}   5)      5*      5+      5,      
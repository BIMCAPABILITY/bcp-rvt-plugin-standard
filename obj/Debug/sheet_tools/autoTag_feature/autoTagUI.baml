   M S B A M L   `   `   `  ����
  bimkit2    ,bimkit.sheet_tools.autoTag_feature.autoTagUI   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Aav2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6G     � Title$  AutoTagging Options��5   	    � WindowStyle$
 None=�6%    � AllowsTransparency . $	��340��6R   $	��500��6_   .��5      �� 
���   �$��1q�6!   ._�5   
   � .+�5	      �� -   5        $��40��5	      $	��Top=�6!   ��5
      �� 5      .��5      �  � Color$ #3264c8��6'    � Offset$ 0Q�67   � 5      $ #090b3c��6'   $ 1Q�67   5      ._�5      � .+�5      �� $��BIMKit��6    
�������$��20(�6A   $��Center=�6O   $��10,0,0,0q�6j   �� 5      -   5        $��X��5      $��20��6)   $��20��64   $��Right=�6@   $��Center=�6\   $��0,0,10,0q�6w   
������ 
�������
������ 6�   5      5      �� 5      $��Right=�6   $��	0,45,30,0q�64   $
��Wrap=�6G   $��Version: 1.2.0��6[   $	��Top=�6q   �� 5      $��Right=�6   $��	0,61,30,0q�64   $
��Wrap=�6G   $*��$Resolve 2D tag clashes in plan views��6[   $	��Top=�6�   �� 5      $��30,70,30,20q�6   $	��Top=�6.   .+�5      �� $��Select Tag Categories:��6   $��14(�6:   $
��Bold"�6H   �� 5      -   5         ��Name$ CategoryListBox��5      $	��100��63   $��0,10,120,10q�6@   5      �� 5      -   5        $��SelectAllButton��5      $��Deselect All��60   $��Right=�6G   $	��100��6c   $��30��6o   $��
0,-40,0,10q�6{   6�   �� 5!      $��Clash Fix Iterations:��6   $��14(�69   $
��Bold"�6G   �� 5#       �� Orientation$ 
Horizontal=�6   $��0,5,0,0q�66   $��Center=�6G   .+�5%      �� $��13��6    $
��Left=�6d   $	��Top=�6   $
��Bold"�6�   $��0,0,0,0q�6�   ��6+   ��  �� ElementName$ IterationSlider�� �� Path$ Value�6�   �� 5'      -   5        	 ��Name$	 IterationSlider��5'      
 � Minimum$
 0Q�66    � Maximum$ 10Q�6B    � Value$ 3Q�6O    �� TickFrequency$ 1Q�6Y    �� IsSnapToTickEnabled . $	��375��6�   �� 5)      $��50��6    $��10 max��6+   $��Right=�69   $	��top=�6U   $
��Bold"�6m   $��0,0,0,0q�6   5*      �� 5,      $=��7Higher iterations = better clash resolution but slower.��6   $��12(�6[   
�������$��0,5,0,15q�6{   �� 5.      -   5        $��Fix Annotation Clashes��5.      $	��150��6:   $��30��6F   $��Right=�6R   
��� � �6�   �� 50      $
��Left=�6   $��	0,-13,0,0q�67   $	��Top=�6J   �� 6c   -   5        $��https://www.bimcap.com/?�50   m   .�6�   �� $�Copyright © 2024 BIMCAP��6�   6�   6�   51      52      53      54      
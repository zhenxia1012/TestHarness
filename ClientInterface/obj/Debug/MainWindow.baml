   M S B A M L   `   `   `  ����J  FClientInterface, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:ClientInterfaceClientInterface  5   	        ClientInterface.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:ClientInterface     5   	     � Title$  TestHarness��5   	   $	��626��6   $	��655��6*   .��5	      �  � RowDefinitions 5
   
   �� 5      $��40 �6   �� 5      $��40 �6   �� 5      $��40 �6   �� 5      $��40 �6   �� 5      $��* �6   .+�5   
   �� $��1��6    �� Orientation$ 
Horizontal=�6"   .+�5      �� -   5        $��sendll��5   /   $��23��6   $
��40,0q�6!   $��Center=�6=   $	��100��5      $��Send Dll��60   �� 5      -   5        $��sendtestrequest��5   /   $��23��6   $
��90,0q�6!   $��Center=�6F   $	��100��5      $��SendTestRequest��69   �� 5      -   5        $��Query��5   /   $��23��6   $
��60,0q�6!   $��Center=�6<   $	��100��5      $��Query��6   6,   5      �� 5   
   $��2��6   $ 
Horizontal=�6"   .+�5      �� -   5        $��connect��5   /   $��23��6   $
��40,0q�6!   $��Center=�6>   $	��100��5      $��Connect��6   6-   �� 5      -   5        $��
disconnect��5   /   $��23��6   $
��10,0q�6!   $��Center=�6A   $	��100��5      $��
Disconnect��6   61   �� 5      -   5        $
��send��5   /   $��23��6   $
��40,0q�6!   $��Center=�6;   $	��100��5      $
��Send��6   6+   �� 5       -   5        $��generate��5    /   $��23��6   $
��40,0q�6!   $��Center=�6?   $	��100��5!      $��Generate��6   6.   5"      �� 5#   
   $��3��6   $ 
Horizontal=�6"   .+�5$      �� -   5        $��yeartext��5$   I   $��3��6   $��28��6#   $	��100��6/   $
��60,0q�6;   $��Center=�6Y   .��6t   5%      �� 5&      -	   5        $��	yearlabel��5&   F   $��3��6   $
��-100q�6!   $��28��6/   $��50��6;   $��Center=�6W   $
��Year��6r   �� 5'      -
   5        $��	monthtext��5'   I   $��3��6   $��28��6#   $	��100��6/   $
��40,0q�6;   $��Center=�6Z   .��6u   5(      �� 5)      -   5        $��
monthlabel��5)   F   $��3��6   $
��-100q�6!   $��28��6/   $��50��6;   $��Center=�6X   $��Month��6s   �� 5*      -   5        $��daytext��5*   I   $��3��6   $��28��6#   $	��100��6/   $
��40,0q�6;   $��Center=�6X   .��6s   5+      �� 5,      -   5        $��daylabel��5,   F   $��3��6   $
��-100q�6!   $��28��6/   $��50��6;   $��Center=�6V   $	��Day��6q   5-      � 5.   
   $��4��6   .+�5/      �� -   5        $��label3��5/   .   $��28��6   $
��30,0q�6    $��Center=�6<   $��Right=�50      $	��210��6*   $��Center=�66   .��6Z   Received Messages51   $   �� 52      -   5        $��label2��52   .   $��28��6   $
��30,0q�6    $��Center=�6<   $
��Left=�53      $	��210��6)   $��Center=�65   .��6Y   Sent Messages54       55      � 56   
   $��5��6    � ColumnDefinitions 57      �� 58      �� 59      .+�5;      �� -   5        $
��menu��5;   ?   $��0��6   $��-80, 20, 15, 20q�6&   $	��180��6K   5=      �� 5>      -   5        $��result��5>   @   $��1��6   $��-100, 20, 30, 20q�6&   $	��350��6N   5?      5@      5A      
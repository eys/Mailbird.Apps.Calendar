   M S B A M L   `   `   `  ����Q  MMailbird.Apps.Calendar, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullU/clr-namespace:Mailbird.Apps.Calendar.ViewModels!Mailbird.Apps.Calendar.ViewModels  5      '    !Mailbird.Apps.Calendar.MainWindow   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      a ]DevExpress.Xpf.Core.v14.1, Version=14.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49al hDevExpress.Xpf.Core.v14.1.Extensions, Version=14.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a=dx2http://schemas.devexpress.com/winfx/2008/xaml/core   5      f bDevExpress.Xpf.Scheduler.v14.1, Version=14.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49aCdxsch7http://schemas.devexpress.com/winfx/2008/xaml/scheduler  5      ?dxe5http://schemas.devexpress.com/winfx/2008/xaml/editors  5      ?	viewModel/clr-namespace:Mailbird.Apps.Calendar.ViewModels  5        � Title$  Mailbird Calendar��5	      $	��600��6   $	��900��6,   &   DevExpress.Xpf.Core.ThemeManager   	ThemeName0  *DevExpress.Xpf.Core.ThemeNameTypeConverter$ MetropolisLight 5
       � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $  UIStyles/GeneralStyle.xaml?�6%   �� 5      $# UIStyles/AppointmetStyle.xaml?�6%   �� 5      $&  UIStyles/FlyoutContentStyle.xaml?�6%   �� 5      $) #UIStyles/SchedulerContentStyle.xaml?�6%   %    5       � DataContext 5      ;   5Mailbird.Apps.Calendar.ViewModels.MainWindowViewModel  5   
   .��5      � -   5         �Name$ 
LayoutRoot��5       � RowDefinitions 5    
   �� 5!      $
��Auto �6   �� 5"      $��* �6   .+�5%   
   � $��0��6   .+�5&      ��  �� Orientation$ 
Horizontal=�6   $
��Left=�5'      .+�5(      �� -   5        	 ��Name$
	 Menu��5(      $��25��5)      $��25��5*      $��3q�5+      .��5-      �� $��25��6   $��25��5.      $��	-1,-1,0,0q�5/      .+�50      �� 
�������$��2��51      	
 �� X1$
 5��52      	 �� Y1$ 8��53      	 �� X2$ 20��54      	 �� Y2$ 8��55      �� 57      
�������$��2��58      $
 5��59      $ 13��5:      $ 20��5;      $ 13��5<      �� 5>      
�������$��2��5?      $
 5��5@      $ 18��5A      $ 20��5B      $ 18��5C      5D      5E      %  DevExpress.Xpf.Editors.DateEdit  5G      -   5          Name$ DateSelector��5G      %  DevExpress.Xpf.Editors.TextEdit   Mask$ y��5H         DateSelectorStyle#��[  5I         DateTime 5J      �� *Start+ �� ElementName$ 	Scheduler��%  DevExpress.Xpf.Editors.BaseEdit   StyleSettings 5L      8  2DevExpress.Xpf.Editors.DateEditPickerStyleSettings  5M      -   5          Name$ DateEditPicker��5M   :   5O      5P      �� 5R      -   5         ��Name$ 	Selectors��5T      $ 
Horizontal=�5R      $��Right=�5S      .+�5U      �� -   5         ��Name$ SelectorList��5U      $��Disabled=�60   $��3q�6f     SelectorViewStyle#��[ 5V       �� SelectionMode$ Single=�6E    �� SelectedIndex 5X      �� *ActiveViewType+ �� ConverterParameter$ SelectorList��  ListItemToViewTypeConverter �� 	Converter# [ $ 	Scheduler�� �� Mode$ TwoWay=�.@�5Y      �� $	��Day��6"   �� 5[      $
��Week��6"   �� 5]      $��Month��6"   �� 5_      $��	Work Week��6"   5`      5a      5b      � 5c   
   $��1��6   .+�5d      1  +DevExpress.Xpf.Editors.Flyout.FlyoutControl  -   5          Name$ FlyoutControl��5d       5	  /DevExpress.Xpf.Editors.Flyout.Native.FlyoutBase 	  	StaysOpen .  	 Right#��[ 5i        	  AllowMoveAnimation .  5e       �� *FlyoutViewModel+ 	  PlacementTarget 5f       �� $ 	Scheduler�� 	  IsOpen 5h       ��   �� Path$  IsOpen�$ TwoWay=�5j       /
  )DevExpress.Xpf.Scheduler.SchedulerControl
  5l      -   5        ! 
 Name$! 	Scheduler��5l   %   $��3q�5m   %   $��Stretch=�5n   %   " 
  ActiveViewType$" Month=�5o   %   ## 
  SelectedAppointmentsBindable# 5p   %   �� *&$FlyoutViewModel.SelectedAppointments+$ 
  Storage$ 5s      /  )DevExpress.Xpf.Scheduler.SchedulerStorage  5t      -	   5        %   AppointmentStorage% 5v      1  +DevExpress.Xpf.Scheduler.AppointmentStorage  5w      �  �DevExpress.Xpf.Scheduler.PersistentObjectStorageBase`2[[DevExpress.Xpf.Scheduler.Native.AppointmentDataStorage, DevExpress.Xpf.Scheduler.v14.1, Version=14.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a],[DevExpress.XtraScheduler.Appointment, DevExpress.XtraScheduler.v14.1.Core, Version=14.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a]]&   
DataSource& 67   �� *AppointmentCollection+'   Mappings' 5x   "   1  +DevExpress.Xpf.Scheduler.AppointmentMapping  5y   &   (   AppointmentId$( Id��6?   )   Start$) 	StartTime��5z   ?   
*   End$* EndTime��5{   ?   +   Subject$+ Subject��5|   ?   ,   Location$, Location��5}   ?   -   
ResourceId$- 
ResourceId��5~   ?   .   Label$. LabelId��5   ?   /   Status$/ StatusId��5�   ?   0   Description$0 Description��5�   ?   1   ReminderInfo$1 ReminderInfo��5�   ?   2   CustomFieldMappings2 5�   &   :  4DevExpress.Xpf.Scheduler.SchedulerCustomFieldMapping  3   Member$3 Calendar��6H   4   Name$4 
cfCalendar��5�   H   5   	ValueType$5 Object=�5�   H   5�      6   ResourceStorage6 5�      .  (DevExpress.Xpf.Scheduler.ResourceStorage  5�      7   Mappings7 5�   "   .  (DevExpress.Xpf.Scheduler.ResourceMapping  5�   &   -
   5        8  Name$
8 Name��5�   D   	9   Id$9 Id��6<   6D   5�      5�      5�      5�      5�      5�      
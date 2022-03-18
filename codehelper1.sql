declare @ccode nvarchar(100)
set @ccode='6601'
delete from code where ccode like @ccode+'%' and ccode not in (@ccode,@ccode+'01')
insert into code
(cclass, cclass_engl, cclassany, cclassany_engl,ccode,ccode_name, ccode_engl, igrade, bproperty, cbook_type, cbook_type_engl, chelp, cexch_name, cmeasure, bperson, bcus, bsup, bdept, bitem, cass_item, br, be, cgather, bend, bexchange, bcash, bbank, bused, bd_c, dbegin, dend, itrans, bclose, cother, iotherused, bcDefine1, bcDefine2, bcDefine3, bcDefine4, bcDefine5, bcDefine6, bcDefine7, bcDefine8, bcDefine9, bcDefine10, iViewItem, bGCJS, bCashItem, bcDefine11, bcDefine12, bcDefine13, bcDefine14, bcDefine15, bcDefine16, bReport, cUserDefineType, iyear, dModifyDate, bparacc)
select 
cclass, cclass_engl, cclassany, cclassany_engl,@ccode+a.ccode ccode,a.cname ccode_name, ccode_engl, igrade, bproperty, cbook_type, cbook_type_engl, chelp, cexch_name, cmeasure, bperson, bcus, bsup, bdept, bitem, cass_item, br, be, cgather, bend, bexchange, bcash, bbank, bused, bd_c, dbegin, dend, itrans, bclose, cother, iotherused, bcDefine1, bcDefine2, bcDefine3, bcDefine4, bcDefine5, bcDefine6, bcDefine7, bcDefine8, bcDefine9, bcDefine10, iViewItem, bGCJS, bCashItem, bcDefine11, bcDefine12, bcDefine13, bcDefine14, bcDefine15, bcDefine16, bReport, cUserDefineType, iyear, dModifyDate, bparacc
from
(select '02' ccode,'ְ��������' cname union all
select '05' ccode,'ְ����������' cname union all
select '07' ccode,'�д���' cname union all
select '08' ccode,'�칫��' cname union all
select '09' ccode,'IT�Ĳ�' cname union all
select '10' ccode,'���÷�' cname union all
select '11' ccode,'��ͨ��' cname union all
select '12' ccode,'ͨѶ��' cname union all
select '13' ccode,'�г�������' cname union all
select '14' ccode,'����' cname union all
select '15' ccode,'װ�޷�' cname union all
select '16' ccode,'ˮ����ҵ' cname union all
select '18' ccode,'�����' cname union all
select '19' ccode,'�����' cname union all
select '20' ccode,'�н������' cname union all
select '23' ccode,'  ��Ͷ�����' cname union all
select '24' ccode,'  ���޷�' cname union all
select '25' ccode,'  ��Ƹ��' cname union all
select '26' ccode,'  �б���' cname) a,
(select * from code where ccode=@ccode+'01') b
--delete from code where len(ccode)=2


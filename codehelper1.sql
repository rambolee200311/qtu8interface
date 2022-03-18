declare @ccode nvarchar(100)
set @ccode='6601'
delete from code where ccode like @ccode+'%' and ccode not in (@ccode,@ccode+'01')
insert into code
(cclass, cclass_engl, cclassany, cclassany_engl,ccode,ccode_name, ccode_engl, igrade, bproperty, cbook_type, cbook_type_engl, chelp, cexch_name, cmeasure, bperson, bcus, bsup, bdept, bitem, cass_item, br, be, cgather, bend, bexchange, bcash, bbank, bused, bd_c, dbegin, dend, itrans, bclose, cother, iotherused, bcDefine1, bcDefine2, bcDefine3, bcDefine4, bcDefine5, bcDefine6, bcDefine7, bcDefine8, bcDefine9, bcDefine10, iViewItem, bGCJS, bCashItem, bcDefine11, bcDefine12, bcDefine13, bcDefine14, bcDefine15, bcDefine16, bReport, cUserDefineType, iyear, dModifyDate, bparacc)
select 
cclass, cclass_engl, cclassany, cclassany_engl,@ccode+a.ccode ccode,a.cname ccode_name, ccode_engl, igrade, bproperty, cbook_type, cbook_type_engl, chelp, cexch_name, cmeasure, bperson, bcus, bsup, bdept, bitem, cass_item, br, be, cgather, bend, bexchange, bcash, bbank, bused, bd_c, dbegin, dend, itrans, bclose, cother, iotherused, bcDefine1, bcDefine2, bcDefine3, bcDefine4, bcDefine5, bcDefine6, bcDefine7, bcDefine8, bcDefine9, bcDefine10, iViewItem, bGCJS, bCashItem, bcDefine11, bcDefine12, bcDefine13, bcDefine14, bcDefine15, bcDefine16, bReport, cUserDefineType, iyear, dModifyDate, bparacc
from
(select '02' ccode,'职工福利费' cname union all
select '05' ccode,'职工教育经费' cname union all
select '07' ccode,'招待费' cname union all
select '08' ccode,'办公费' cname union all
select '09' ccode,'IT耗材' cname union all
select '10' ccode,'差旅费' cname union all
select '11' ccode,'交通费' cname union all
select '12' ccode,'通讯费' cname union all
select '13' ccode,'市场宣传费' cname union all
select '14' ccode,'房租' cname union all
select '15' ccode,'装修费' cname union all
select '16' ccode,'水电物业' cname union all
select '18' ccode,'服务费' cname union all
select '19' ccode,'会务费' cname union all
select '20' ccode,'中介机构费' cname union all
select '23' ccode,'  招投标费用' cname union all
select '24' ccode,'  租赁费' cname union all
select '25' ccode,'  招聘费' cname union all
select '26' ccode,'  残保金' cname) a,
(select * from code where ccode=@ccode+'01') b
--delete from code where len(ccode)=2


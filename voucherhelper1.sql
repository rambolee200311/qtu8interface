use UFDATA_996_2020
select * from hr_hi_person where cdept_num='1' or cDept_num='4'
select cDept_num from hr_hi_person where cPsn_Name=''
select * from fitemss97
select citemcode from fitemss97 where citemname=''
select * from Department
select  cPsn_Num from hr_hi_person
--select * from gl_accsum
/*
declare @ccode nvarchar(100)
set @ccode='6601'
update code set bdept=1 where ccode like @ccode+'%' and ccode not in (@ccode,@ccode+'01')
update code set bitem=1,cass_item='97' where ccode like @ccode+'%' and ccode not in (@ccode,@ccode+'01')

insert into UFSystem..UA_Account_sub 
SELECT '996'cAcc_Id, iYear, cSub_Id, bIsDelete, bClosing, iModiPeri, dSubSysUsed, cUser_Id, dSubOriDate
 FROM UFSystem..UA_Account_sub WHERE cAcc_Id='998' AND cSub_Id='GC'
 update UFSystem..UA_Account_sub set dSubSysUsed=null WHERE cAcc_Id='996' AND cSub_Id='GC' AND iYear='9999' 

 select iprice,* from gl_accvouch

 alter table gl_accvouch add iprice decimal
*/
SELECT * FROM UFSystem..UA_Account_sub WHERE cSub_Id='GC'
SELECT * FROM UFSystem..UA_Account_sub WHERE cAcc_Id='996' AND cSub_Id='GC' AND iYear='9999' AND dSubSysUsed IS NOT NULL

select * from UA_Menu where cMenu_Name like '%实施%'

select convert(nvarchar(10),iyear)+'年'+convert(nvarchar(10),iperiod)+'月'+csign+convert(nvarchar(10),ino_id) result from gl_accvouch where cdigest like '%A8974922%' and isnull(iflag,0)=0
set nocount on

select
[Username],
[UserDomain],
[Name],
[Title],
[Process],
[Start],
iif(lead([Title]) over (partition by [Username] order by [Start], [Name])='<Start>', Null, [End]) as 'End',
iif(lead([Title]) over (partition by [Username] order by [Start], [Name])='<Start>', Null, [Duration]) as 'dd:hh:mm:ss'
from (
select
iif(CHARINDEX('\', [Username])>0, REPLACE(SUBSTRING([Username], CHARINDEX('\', [Username]), LEN([Username])), '\', ''), [Username]) as 'UserName',
iif(CHARINDEX('\', [Username])>0, LEFT([Username], CHARINDEX('\', [Username]) - 1), null) as 'UserDomain',
[Name],
[WinTitle] as 'Title',
[WinProcess] as 'Process',
[Start],
[End],
--http://www.sqlteam.com/article/working-with-time-spans-and-durations-in-sql-server
right('0' + rtrim(ElapsedSecs / 86400), 4) + ':' +
right('0' + rtrim((ElapsedSecs % 86400) / 3600), 2) + ':' +
right('0' + rtrim((ElapsedSecs % 3600) / 60), 2) + ':' +
right('0' + rtrim(ElapsedSecs % 60), 2)
as 'Duration'
From (
select
*,
datediff(second, [Start], [End]) as 'ElapsedSecs'
from (
select
psy.IDx,
psy.UserName,
iif(CHARINDEX('\', psy.Name)>0, REPLACE(SUBSTRING(psy.Name, CHARINDEX('\', psy.Name), LEN(psy.Name)), '\', ''), psy.Name) as 'Name',
psy.WinTitle,
psy.WinProcess,
psy.TimeLogged as 'Start',
lead(psy.TimeLogged) over (partition by psy.UserName order by psy.TimeLogged, psy.Name) as 'End'
from PsyMain psy with (NoLock)
) xx
) yy
) zz
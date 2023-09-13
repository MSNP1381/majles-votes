SELECT
	title as session_title,
	Against as	'Session Against',
	Favor as	'Session Favor',
	Abstaining as 'Session Abstaining',
	Name,
	Family,
	ImageUrl,
	Region,
	Value as Activity,
	v.jdate 
FROM
	Votes v
	LEFT JOIN VotingSessions vs ON v.VotingSessionId = vs.Id
	LEFT JOIN AttendeceTypes at ON at."Key" = v.activity
	LEFT JOIN Members AS m ON m.Id = v.MemberId
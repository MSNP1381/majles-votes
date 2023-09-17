SELECT
	MemId,
	title as session_title,
	vs.Id as session_id ,
	Against as	'Session Against',
	Favor as	'Session Favor',
	Abstaining as 'Session Abstaining',
	Name,
	Family,
	ImageUrl,
	Region,
	at.type_value Activity,
	v.jdate ,
	m.FirstVote,
FROM
	Votes v
	LEFT JOIN VotingSessions vs ON v.VotingSessionId = vs.Id
	LEFT JOIN AttendeceTypes at ON at.type_key = v.activity
	LEFT JOIN Members AS m ON m.Id = v.MemberId

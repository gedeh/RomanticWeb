﻿DELETE WHERE
{{
	GRAPH @graph 
	{{ 
		{0}
	}}
}};

INSERT DATA
{{ 
	GRAPH @graph 
	{{
		{1} 
	}}

	GRAPH @metaGraph
	{{
		@graph <http://xmlns.com/foaf/0.1/primaryTopic> @entity .
	}}
}};
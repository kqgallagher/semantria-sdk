var fs = require('fs');
var assert = require("assert");
var SemantriaSession = require("../").Session;
var config = require('../test-config');
try { config = require('../test-config.override') } catch(e) {}

describe('semantria-sdk [promise]', function() {
    var new_config = Object.assign({}, config);
    new_config.consumerKey = config.consumerKey || process.env.SEMANTRIA_KEY;
    new_config.consumerSecret = config.consumerSecret || process.env.SEMANTRIA_SECRET;
	var session = new SemantriaSession(new_config, "test"),
		config_id = false;

	this.timeout(20000);
	this.slow(10000);

	before(function(  ) {
		return session.addConfigurations([{
			name: "NODEJS_TEST_CONFIG",
			is_primary: false,
			auto_response: false,
			language: "English"
		}]).then(function (result) {
			if(result instanceof Array && result[0] instanceof Object) {
				config_id = result[0].id;
			}
		});
	});

	after(function( done ) {
		session.removeConfigurations([config_id]).then(function() {
			config_id = false;
			done();
		}).catch(done);
	});

	it('getStatistics()', function ( done ) {
		var async_wait = true;
		var params = {
			interval: 'Month'
		}
		session.getStatistics(params).then(function(results) {
			async_wait = false;
			assert.ok( results instanceof Array && results.length > 0);
			var statistics = results[0];
			assert.ok( statistics instanceof Object  && typeof statistics.docs_queued !== "undefined" );
			done();
		}).catch(done);
		assert.ok(async_wait, 'synchronous call for getStatistics()');
	});

	it('getSubscription()', function ( done ) {
		var async_wait = true;
		session.getSubscription(true).then(function(subscription){
			async_wait = false;
			assert.ok( subscription instanceof Object  && typeof subscription.name != "undefined" );
			done();
		}).catch(done);
		assert.ok(async_wait, 'synchronous call for getSubscription()');
	});

	it( "getStatus()", function( done ) {
		var async_wait = true;
		session.getStatus(true).then(function(status){
			async_wait = false;
			assert.ok( status instanceof Object && typeof status.api_version != "undefined" );
			done();
		}).catch(done);
		assert.ok(async_wait, 'synchronous call for getStatus()');
	});

	it( "getSupportedFeatures()", function( done ) {
		var async_wait = true;
		session.getSupportedFeatures("English").then(function(features){
			async_wait = false;
			assert.ok( features instanceof Array && features[0].language == "English" );
			done();
		}).catch(done);
		assert.ok(async_wait, 'synchronous call for getSupportedFeatures()');
	});

	describe('[get/add/update/remove] Configurations', function () {
		var configuration = null,
			configuration_id = false,
			configuration_name = "Test configuration",
			configuration2 = null,
			configuration2_id = false,
			configuration2_name = "Test configuration - clone";

		it('getConfigurations()', function ( done ) {
			var async_wait = true;
			session.getConfigurations(true).then(function(configurations){
				async_wait = false;
				assert.ok( configurations instanceof Array);
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for getConfigurations()');
		});

		it('addConfigurations()', function ( done ) {
			var async_wait = true;
			session.addConfigurations( [{
				name: configuration_name,
				is_primary: false,
				auto_response: false,
				language: "English"
			}], true ).then(function(result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object && typeof result[0].id === 'string',
					'addConfigurations() response'
				);
				configuration = result[0];
				delete configuration.timestamp;
				configuration_id = configuration.id

				//check: configuration exists
				return session.getConfigurations(true);
			}).then(function(configurations){
				var find_configuration = false;
				for (var i=0; i<configurations.length; i++) {
					if (configurations[i].id == configuration_id) {
						find_configuration = configurations[i];
					}
				}
				assert.ok( find_configuration instanceof Object, "test configuration does not exist" );
				assert.strictEqual(find_configuration.name, configuration_name, "test configuration has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for addConfigurations()');
		});

		it('updateConfigurations()', function ( done ) {
			if (!configuration_id) throw 'Test item not was added';
			var async_wait = true;
			configuration_name = "Test configuration - renamed";
			var update = {
				id: configuration.id,
				name: configuration_name
			};
			session.updateConfigurations([update]).then(function (result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object,
					"updateConfigurations() response"
				);

				//check: configuration was updated
				return session.getConfigurations(true);
			}).then(function(configurations){
				var find_configuration = false;
				for (var i=0; i<configurations.length; i++) {
					if (configurations[i].id == configuration_id) {
						find_configuration = configurations[i];
					}
				}
				assert.ok( find_configuration instanceof Object, "test configuration does not exist" );
				assert.strictEqual(find_configuration.name, configuration_name, "test configuration has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for updateConfigurations()');

		});

		it('cloneConfiguration()', function ( done ) {
			if (!configuration_id) throw 'Test item not was added';
			var async_wait = true;
			configuration_name = "Test configuration - renamed";
			configuration.name = configuration_name;
			session.cloneConfiguration(configuration2_name, configuration_id).then(function (result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object && typeof result[0].id === 'string',
					'cloneConfiguration() response'
				);
				configuration2 = result[0];
				delete configuration2.timestamp;
				configuration2_id = configuration2.id

				//check: configuration exists
				return session.getConfigurations(true);
			}).then(function(configurations){
				var find_configuration = false;
				for (var i=0; i<configurations.length; i++) {
					if (configurations[i].id == configuration2_id) {
						find_configuration = configurations[i];
					}
				}
				assert.ok( find_configuration instanceof Object, "test configuration does not exist" );
				assert.strictEqual(find_configuration.name, configuration2_name, "test configuration has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for updateConfigurations()');

		});

		it('removeConfigurations()', function ( done ) {
			if (!configuration_id) throw 'Test item not was added';
			var async_wait = true;
			var ids = [configuration.id];
			if (configuration2_id) ids.push(configuration2_id);
			session.removeConfigurations(ids).then(function (result) {
				async_wait = false;
				assert.equal(result, 202, "removeConfigurations() response");

				//check: configuration was removed
				return session.getConfigurations(true);
			}).then(function(configurations){
				var find_configuration = false;
				for (var i=0; i<configurations.length; i++) {
					if (configurations[i].id == configuration_id) {
						find_configuration = configurations[i];
					}
				}
				assert.strictEqual(find_configuration, false, "test configuration was not removed");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for removeConfigurations()');
		});
	});

	describe('[get/add/update/remove] Blacklist', function () {
		var blacklist = null,
			blacklist_id = false,
			blacklist_name = "test blacklist";

		it('getBlacklist()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist'
			var async_wait = true;
			session.getBlacklist(config_id).then(function (blacklists) {
				async_wait = false;
				assert.equal(blacklists, 202, "getBlacklist() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for getBlacklist()');
		});

		it('addBlacklist()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.addBlacklist([{
					name: blacklist_name
				}], config_id).then(function (result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object && typeof result[0].id === 'string',
					"addBlacklist() response"
				);
				blacklist = result[0];
				blacklist_id = blacklist.id
				delete blacklist.timestamp;

				//check: blacklist exists
				return session.getBlacklist(config_id);
			}).then(function (blacklists) {
				assert.ok(blacklists instanceof Array, "getBlacklist() is expected to return an array");
				assert.strictEqual(blacklists.length, 1, "result array is expected to contain one item");
				assert.strictEqual(blacklists[0].id, blacklist_id, "test item has not correct id");
				assert.strictEqual(blacklists[0].name, blacklist_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for addBlacklist()');
		});

		it('updateBlacklist()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!blacklist_id) throw 'Test item not was added';
			var async_wait = true;
			blacklist_name = "test blacklist - renamed";
			var update = {
				id: blacklist.id,
				name: blacklist_name
			};
			session.updateBlacklist([update], config_id).then(function (result) {
				async_wait = false;
				assert.ok( result instanceof Array && result[0] instanceof Object, "updateBlacklist() response" );

				//check: blacklist was updated
				return session.getBlacklist(config_id);
			}).then(function (blacklists) {
				assert.ok(blacklists instanceof Array, "getBlacklist() is expected to return an array");
				assert.strictEqual(blacklists.length, 1, "result array is expected to contain one item");
				assert.strictEqual(blacklists[0].id, blacklist_id, "test item has not correct id");
				assert.strictEqual(blacklists[0].name, blacklist_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for updateBlacklist()');
		});

		it('removeBlacklist()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!blacklist_id) throw 'Test item not was added';
			var async_wait = true;
			session.removeBlacklist([blacklist.id], config_id).then(function (result) {
				async_wait = false;
				assert.equal(result, 202, "removeBlacklist() response");
				return session.getBlacklist(config_id);
			}).then(function (blacklists) {
				assert.equal(blacklists, 202, "getBlacklist() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for removeBlacklist()');
		});
	});

	describe('[get/add/update/remove] Categories', function () {
		var category = null,
			category_id = false,
			category_name = "test category";

		it('getCategories()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist'
			var async_wait = true;
			session.getCategories(config_id).then(function (categories) {
				async_wait = false;
				assert.equal(categories, 202, "getCategories() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for getCategories()');
		});

		it('addCategories()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.addCategories([{
					name: category_name,
					weight: 0.75,
					samples: ["EC2", "AWS"]
				}], config_id).then(function (result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object && typeof result[0].id === 'string',
					"addCategories() response"
				);
				category = result[0];
				category_id = category.id
				delete category.timestamp;

				//check: category exists
				return session.getCategories(config_id);
			}).then(function (categories) {
				assert.ok(categories instanceof Array, "getCategories() is expected to return an array");
				assert.strictEqual(categories.length, 1, "result array is expected to contain one item");
				assert.strictEqual(categories[0].id, category_id, "test item has not correct id");
				assert.strictEqual(categories[0].name, category_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for addCategories()');
		});

		it('updateCategories()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!category_id) throw 'Test item not was added';
			var async_wait = true;
			category_name = "test category - renamed";
			var update = {
				id: category.id,
				name: category_name
			};
			session.updateCategories([update], config_id).then(function (result) {
				async_wait = false;
				assert.ok( result instanceof Array && result[0] instanceof Object, "updateCategories() response" );

				//check: category was updated
				return session.getCategories(config_id);
			}).then(function (categories) {
				assert.ok(categories instanceof Array, "getCategories() is expected to return an array");
				assert.strictEqual(categories.length, 1, "result array is expected to contain one item");
				assert.strictEqual(categories[0].id, category_id, "test item has not correct id");
				assert.strictEqual(categories[0].name, category_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for updateCategories()');
		});

		it('removeCategories()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!category_id) throw 'Test item not was added';
			var async_wait = true;
			session.removeCategories([category.id], config_id).then(function (result) {
				async_wait = false;
				assert.equal(result, 202, "removeCategories() response");
				return session.getCategories(config_id);
			}).then(function (categories) {
				assert.equal(categories, 202, "getCategories() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for removeCategories()');
		});
	});

	describe('[get/add/update/remove] Queries', function () {
		var query = null,
			query_id = false,
			query_name = "test query",
			query_query = "Amazon AND (EC2 OR AWS)";

		it('getQueries()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist'
			var async_wait = true;
			session.getQueries(config_id).then(function (queries) {
				async_wait = false;
				assert.equal(queries, 202, "getQueries() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for getQueries()');
		});

		it('addQueries()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.addQueries([{
					name: query_name,
					query: query_query
				}], config_id).then(function (result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object && typeof result[0].id === 'string',
					"addQueries() response"
				);
				query = result[0];
				query_id = query.id
				delete query.timestamp;

				//check: query exists
				return session.getQueries(config_id);
			}).then(function (queries) {
				assert.ok(queries instanceof Array, "getQueries() is expected to return an array");
				assert.strictEqual(queries.length, 1, "result array is expected to contain one item");
				assert.strictEqual(queries[0].id, query_id, "test item has not correct id");
				assert.strictEqual(queries[0].name, query_name, "test item has not correct name");
				assert.strictEqual(queries[0].query, query_query, "test item has not correct query");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for addQueries()');
		});

		it('updateQueries()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!query_id) throw 'Test item not was added';
			var async_wait = true;
			query_query = "Amazon AND (EC2 OR ec2 OR AWS OR aws)";
			var update = {
				id: query.id,
				query: query_query
			};
			session.updateQueries([update], config_id).then(function (result) {
				async_wait = false;
				assert.ok( result instanceof Array && result[0] instanceof Object, "updateQueries() response" );

				//check: query was updated
				return session.getQueries(config_id);
			}).then(function (queries) {
				assert.ok(queries instanceof Array, "getQueries() is expected to return an array");
				assert.strictEqual(queries.length, 1, "result array is expected to contain one item");
				assert.strictEqual(queries[0].id, query_id, "test item has not correct id");
				assert.strictEqual(queries[0].name, query_name, "test item has not correct name");
				assert.strictEqual(queries[0].query, query_query, "test item has not correct query");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for updateQueries()');
		});

		it('removeQueries()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!query_id) throw 'Test item not was added';
			var async_wait = true;
			session.removeQueries([query.id], config_id).then(function (result) {
				async_wait = false;
				assert.equal(result, 202, "removeQueries() response");
				return session.getQueries(config_id);
			}).then(function (queries) {
				assert.equal(queries, 202, "getQueries() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for removeQueries()');
		});
	});

	describe('[get/add/update/remove] Entities', function () {
		var entity = null,
			entity_id = false,
			entity_name = "test entity";

		it('getEntities()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist'
			var async_wait = true;
			session.getEntities(config_id).then(function (entities) {
				async_wait = false;
				assert.equal(entities, 202, "getEntities() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for getEntities()');
		});

		it('addEntities()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.addEntities([{
					name: entity_name,
					type: "furniture"
				}], config_id).then(function (result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object && typeof result[0].id === 'string',
					"addEntities() response"
				);
				entity = result[0];
				entity_id = entity.id
				delete entity.timestamp;

				//check: entity exists
				return session.getEntities(config_id);
			}).then(function (entities) {
				assert.ok(entities instanceof Array, "getEntities() is expected to return an array");
				assert.strictEqual(entities.length, 1, "result array is expected to contain one item");
				assert.strictEqual(entities[0].id, entity_id, "test item has not correct id");
				assert.strictEqual(entities[0].name, entity_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for addEntities()');
		});

		it('updateEntities()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!entity_id) throw 'Test item not was added';
			var async_wait = true;
			entity_name = "test entity - renamed";
			var update = {
				id: entity.id,
				name: entity_name
			};
			session.updateEntities([update], config_id).then(function (result) {
				async_wait = false;
				assert.ok( result instanceof Array && result[0] instanceof Object, "updateEntities() response" );

				//check: entity was updated
				return session.getEntities(config_id);
			}).then(function (entities) {
				assert.ok(entities instanceof Array, "getEntities() is expected to return an array");
				assert.strictEqual(entities.length, 1, "result array is expected to contain one item");
				assert.strictEqual(entities[0].id, entity_id, "test item has not correct id");
				assert.strictEqual(entities[0].name, entity_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for updateEntities()');
		});

		it('removeEntities()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!entity_id) throw 'Test item not was added';
			var async_wait = true;
			session.removeEntities([entity.id], config_id).then(function (result) {
				async_wait = false;
				assert.equal(result, 202, "removeEntities() response");
				return session.getEntities(config_id);
			}).then(function (entities) {
				assert.equal(entities, 202, "getEntities() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for removeEntities()');
		});
	});

	describe('[get/add/update/remove] Phrases', function () {
		var phrase = null,
			phrase_id = false,
			phrase_name = "test phrase";

		it('getPhrases()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist'
			var async_wait = true;
			session.getPhrases(config_id).then(function (phrases) {
				async_wait = false;
				assert.equal(phrases, 202, "getPhrases() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for getPhrases()');
		});

		it('addPhrases()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.addPhrases([{
					name: phrase_name,
					weight: "0.3"
				}], config_id).then(function (result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object && typeof result[0].id === 'string',
					"addPhrases() response"
				);
				phrase = result[0];
				phrase_id = phrase.id
				delete phrase.timestamp;

				//check: phrase exists
				return session.getPhrases(config_id);
			}).then(function (phrases) {
				assert.ok(phrases instanceof Array, "getPhrases() is expected to return an array");
				assert.strictEqual(phrases.length, 1, "result array is expected to contain one item");
				assert.strictEqual(phrases[0].id, phrase_id, "test item has not correct id");
				assert.strictEqual(phrases[0].name, phrase_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for addPhrases()');
		});

		it('updatePhrases()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!phrase_id) throw 'Test item not was added';
			var async_wait = true;
			phrase_name = "test phrase - renamed";
			var update = {
				id: phrase.id,
				name: phrase_name
			};
			session.updatePhrases([update], config_id).then(function (result) {
				async_wait = false;
				assert.ok( result instanceof Array && result[0] instanceof Object, "updatePhrases() response" );

				//check: phrase was updated
				return session.getPhrases(config_id);
			}).then(function (phrases) {
				assert.ok(phrases instanceof Array, "getPhrases() is expected to return an array");
				assert.strictEqual(phrases.length, 1, "result array is expected to contain one item");
				assert.strictEqual(phrases[0].id, phrase_id, "test item has not correct id");
				assert.strictEqual(phrases[0].name, phrase_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for updatePhrases()');
		});

		it('removePhrases()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!phrase_id) throw 'Test item not was added';
			var async_wait = true;
			session.removePhrases([phrase.id], config_id).then(function (result) {
				async_wait = false;
				assert.equal(result, 202, "removePhrases() response");
				return session.getPhrases(config_id);
			}).then(function (phrases) {
				assert.equal(phrases, 202, "getPhrases() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for removePhrases()');
		});
	});

	describe('[get/add/update/remove] Taxonomy', function () {
		var taxonomy = null,
			taxonomy_id = false,
			taxonomy_name = "test taxonomy";

		it('getTaxonomy()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist'
			var async_wait = true;
			session.getTaxonomy(config_id).then(function (taxonomies) {
				async_wait = false;
				assert.equal(taxonomies, 202, "getTaxonomy() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for getTaxonomy()');
		});

		it('addTaxonomy()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.addTaxonomy([{
					name: taxonomy_name
				}], config_id).then(function (result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object && typeof result[0].id === 'string',
					"addTaxonomy() response"
				);
				taxonomy = result[0];
				taxonomy_id = taxonomy.id
				delete taxonomy.timestamp;

				//check: taxonomy exists
				return session.getTaxonomy(config_id);
			}).then(function (taxonomies) {
				assert.ok(taxonomies instanceof Array, "getTaxonomy() is expected to return an array");
				assert.strictEqual(taxonomies.length, 1, "result array is expected to contain one item");
				assert.strictEqual(taxonomies[0].id, taxonomy_id, "test item has not correct id");
				assert.strictEqual(taxonomies[0].name, taxonomy_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for addTaxonomy()');
		});

		it('updateTaxonomy()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!taxonomy_id) throw 'Test item not was added';
			var async_wait = true;
			taxonomy_name = "test taxonomy - renamed";
			var update = {
				id: taxonomy.id,
				name: taxonomy_name
			};
			session.updateTaxonomy([update], config_id).then(function (result) {
				async_wait = false;
				assert.ok( result instanceof Array && result[0] instanceof Object, "updateTaxonomy() response" );

				//check: taxonomy was updated
				return session.getTaxonomy(config_id);
			}).then(function (taxonomies) {
				assert.ok(taxonomies instanceof Array, "getTaxonomy() is expected to return an array");
				assert.strictEqual(taxonomies.length, 1, "result array is expected to contain one item");
				assert.strictEqual(taxonomies[0].id, taxonomy_id, "test item has not correct id");
				assert.strictEqual(taxonomies[0].name, taxonomy_name, "test item has not correct name");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for updateTaxonomy()');
		});

		it('removeTaxonomy()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			if (!taxonomy_id) throw 'Test item not was added';
			var async_wait = true;
			session.removeTaxonomy([taxonomy.id], config_id).then(function (result) {
				async_wait = false;
				assert.equal(result, 202, "removeTaxonomy() response");
				return session.getTaxonomy(config_id);
			}).then(function (taxonomies) {
				assert.equal(taxonomies, 202, "getTaxonomy() is expected to return empty result");
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for removeTaxonomy()');
		});
	});

	describe('Document functions', function () {
		it('getProcessedDocuments() - empty; ', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.getProcessedDocuments(config_id).then(function (items) {
				async_wait = false;
				assert.equal(items, 202);
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for getProcessedDocuments()');
		});

		it('queueDocument()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.queueDocument({
				id: "TEST_DOCUMENT_1",
				text: "it works"
			}, config_id).then(function (result) {
				async_wait = false;
				assert.equal(result, 202);
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for queueDocument()');
		});

		it('cancelDocument()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.queueDocument({
				id: "TEST_DOCUMENT_2",
				text: "it works"
			}, config_id).then(function() {
				return session.cancelDocument("TEST_DOCUMENT_2", config_id);
			}).then(function(result) {
				async_wait = false;
				assert.ok(result instanceof Object);
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for cancelDocument()');
		});

		it('queueBatchOfDocuments()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.queueBatchOfDocuments([
				{
					id: "TEST_DOCUMENT_3",
					text: "Some text goes here"
				},
				{
					id: "TEST_DOCUMENT_4",
					text: "Some other text goes here"
				}
			], config_id).then(function (result) {
				async_wait = false;
				assert.equal(result, 202);
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for queueBatchOfDocuments()');
		});

		it('getDocument()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			setTimeout(function() {
				var async_wait = true;
				session.getDocument("TEST_DOCUMENT_1", config_id).then(function (result) {
					async_wait = false;
					assert.ok(result instanceof Object);
					done();
				}).catch(done);
				assert.ok(async_wait, 'synchronous call for getDocument()');
			}, 2000); //wait for document-processing
		});

		it('getProcessedDocuments() - after processing test items', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var trys = 4;
			var processed = {};
			var testFn = function() {
				session.getProcessedDocuments(config_id).then(function(items) {
					if (items instanceof Array) {
						for (var i=0; i<items.length; i++) {
							processed[items[i].id] = true;
						}
					}
					if (Object.keys(processed).length >= 3) {
						var ids = items.map(function(i){return i.id});
						assert.ok(processed['TEST_DOCUMENT_1'], 'TEST_DOCUMENT_1 not present in processed documents');
						assert.ok(processed['TEST_DOCUMENT_2'], 'TEST_DOCUMENT_2 not present in processed documents');
						assert.ok(processed['TEST_DOCUMENT_3'], 'TEST_DOCUMENT_3 not present in processed documents');
						assert.ok(processed['TEST_DOCUMENT_4'], 'TEST_DOCUMENT_4 not present in processed documents');
						done();
						return;
					}
					if (trys > 0) {
						trys--;
						setTimeout(testFn, 1000);
						return;
					}
					assert.ok(items instanceof Array, "getProcessedDocuments() is expected to return an array");
					assert.strictEqual(items.length, 3);
					done();
				}).catch(done);
			}
			setTimeout(testFn, 1000);
		});
	});

	describe('Collection functions', function () {
		it('getProcessedCollections() - empty', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.getProcessedCollections(config_id).then(function (items) {
				async_wait = false;
				assert.equal(items, 202);
				done()
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for getProcessedCollections()');
		});

		it('queueCollection()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.queueCollection({
				id: "TEST_COLLECTION_1",
				documents: [
					"it works",
					"it works",
					"it works",
					"it works",
					"it works"
				]
			}, config_id).then(function (result) {
				async_wait = false;
				assert.equal(result, 202);
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for queueCollection()');
		});

		it('cancelCollection()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.queueCollection({
				id: "TEST_COLLECTION_2",
				documents: [
					"it works",
					"it works",
					"it works",
					"it works",
					"it works"
				]
			}, config_id).then(function () {
				return session.cancelCollection("TEST_COLLECTION_2", config_id);
			}).then(function (result) {
				async_wait = false;
				assert.ok(result instanceof Object);
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for cancelCollection()');
		});

		it('getCollection()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			setTimeout(function() {
				var async_wait = true;
				session.getCollection("TEST_COLLECTION_1", config_id).then(function (result) {
					async_wait = false;
					assert.ok(result instanceof Object);
					done();
				}).catch(done);
				assert.ok(async_wait, 'synchronous call for getCollection()');
			}, 2000); //wait for document-processing
		});

		it('getProcessedCollections() - after processing test items', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var trys = 4;
			var testFn = function() {
				session.getProcessedCollections(config_id).then(function (items) {
					if (items instanceof Array && items.length == 2) {
						var ids = items.map(function(i){return i.id});
						assert.notEqual(-1, ids.indexOf('TEST_COLLECTION_1'), 'TEST_COLLECTION_1 not present in processed collections')
						assert.notEqual(-1, ids.indexOf('TEST_COLLECTION_2'), 'TEST_COLLECTION_2 not present in processed collections')
						done();
						return;
					}
					if (trys > 0) {
						trys--;
						setTimeout(testFn, 1000);
						return;
					}
					assert.ok(items instanceof Array, "getProcessedCollections() is expected to return an array");
					assert.strictEqual(items.length, 1);
					done();
				}).catch(done);
			}
			setTimeout(testFn, 1000);
		});
	});

	describe('get salience user directory', function () {
		var query = null,
			query_id = false,
			query_name = "q2",
			query_query = "dog AND cat";

		it('getUserDirectory()', function ( done ) {
			if (!config_id) throw 'Test configuration does not exist';
			var async_wait = true;
			session.addQueries([{
				name: query_name,
				query: query_query
			}], config_id)
			.then(function (result) {
				async_wait = false;
				assert.ok(
					result instanceof Array && result[0] instanceof Object && typeof result[0].id === 'string',
					"addQueries() response"
				);
				query = result[0];
				query_id = query.id
				return session.getUserDirectory(config_id, 'tar');
			})
			.then(function (bytes) {
				assert.ok(bytes.indexOf(query_query) > -1);
				var path = (process.env.TEMP || "/tmp") + "/test.tar";
				fs.writeFile(path, bytes, function(err) {
					if (err) {
						return console.log("Error writing tar file, '%s': %s", path, err);
					}
				});
				done();
			}).catch(done);
			assert.ok(async_wait, 'synchronous call for addQueries()');
		});
	});

})
